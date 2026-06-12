#!/bin/bash
set -e

KEYCLOAK_URL="http://localhost:8080"
ADMIN_USER="${KEYCLOAK_ADMIN_USER:-ahmad}"
ADMIN_PASS="${KEYCLOAK_ADMIN_PASSWORD:-4203874}"
REALM="wednest"
CLIENT_ID="wednest-api"
CLIENT_SECRET="wednest-api-secret-key-2024"

echo "Waiting for Keycloak..."
for i in $(seq 1 60); do
    if curl -sf "$KEYCLOAK_URL/realms/master" > /dev/null 2>&1; then
        echo "Keycloak is ready!"
        break
    fi
    echo "  Attempt $i/60..."
    sleep 2
done

echo "Getting admin token..."
ADMIN_TOKEN=$(curl -sf -X POST "$KEYCLOAK_URL/realms/master/protocol/openid-connect/token" \
    -d "client_id=admin-cli" \
    -d "username=$ADMIN_USER" \
    -d "password=$ADMIN_PASS" \
    -d "grant_type=password" | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)

if [ -z "$ADMIN_TOKEN" ]; then
    echo "ERROR: Failed to get admin token"
    exit 1
fi
echo "Got admin token."

auth() {
    echo "Authorization: Bearer $ADMIN_TOKEN"
}

# Create realm if not exists
echo "Checking realm '$REALM'..."
if ! curl -sf -H "$(auth)" "$KEYCLOAK_URL/admin/realms/$REALM" > /dev/null 2>&1; then
    echo "Creating realm '$REALM'..."
    curl -sf -X POST "$KEYCLOAK_URL/admin/realms" \
        -H "$(auth)" -H "Content-Type: application/json" \
        -d "{
            \"realm\": \"$REALM\",
            \"enabled\": true,
            \"registrationAllowed\": true,
            \"loginWithEmailAllowed\": true
        }"
    echo "Realm created."
else
    echo "Realm '$REALM' already exists."
fi

# Create client if not exists
echo "Checking client '$CLIENT_ID'..."
CLIENT_ID_RESP=$(curl -sf -H "$(auth)" "$KEYCLOAK_URL/admin/realms/$REALM/clients?clientId=$CLIENT_ID")
if echo "$CLIENT_ID_RESP" | grep -q '"clientId"'; then
    echo "Client '$CLIENT_ID' already exists."
else
    echo "Creating client '$CLIENT_ID'..."
    curl -sf -X POST "$KEYCLOAK_URL/admin/realms/$REALM/clients" \
        -H "$(auth)" -H "Content-Type: application/json" \
        -d "{
            \"clientId\": \"$CLIENT_ID\",
            \"enabled\": true,
            \"clientAuthenticatorType\": \"client-secret\",
            \"secret\": \"$CLIENT_SECRET\",
            \"directAccessGrantsEnabled\": true,
            \"standardFlowEnabled\": true,
            \"redirectUris\": [\"http://localhost:3000/*\", \"http://localhost:5000/*\"],
            \"webOrigins\": [\"http://localhost:3000\", \"http://localhost:5000\"],
            \"defaultClientScopes\": [\"openid\", \"profile\", \"email\", \"roles\"]
        }"
    echo "Client created."

    # Get client UUID for adding mappers
    CLIENT_UUID=$(curl -sf -H "$(auth)" "$KEYCLOAK_URL/admin/realms/$REALM/clients?clientId=$CLIENT_ID" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

    # Add audience mapper
    echo "Adding audience mapper..."
    curl -sf -X POST "$KEYCLOAK_URL/admin/realms/$REALM/clients/$CLIENT_UUID/protocol-mappers/models" \
        -H "$(auth)" -H "Content-Type: application/json" \
        -d "{
            \"name\": \"audience\",
            \"protocol\": \"openid-connect\",
            \"protocolMapper\": \"oidc-audience-mapper\",
            \"consentRequired\": false,
            \"config\": {
                \"included.client.audience\": \"$CLIENT_ID\",
                \"id.token.claim\": \"false\",
                \"access.token.claim\": \"true\",
                \"userinfo.token.claim\": \"false\"
            }
        }"

    # Add sub claim mapper
    echo "Adding sub claim mapper..."
    curl -sf -X POST "$KEYCLOAK_URL/admin/realms/$REALM/clients/$CLIENT_UUID/protocol-mappers/models" \
        -H "$(auth)" -H "Content-Type: application/json" \
        -d "{
            \"name\": \"sub-claim\",
            \"protocol\": \"openid-connect\",
            \"protocolMapper\": \"oidc-usermodel-attribute-mapper\",
            \"consentRequired\": false,
            \"config\": {
                \"user.attribute\": \"id\",
                \"id.token.claim\": \"false\",
                \"access.token.claim\": \"true\",
                \"claim.name\": \"sub\",
                \"jsonType.label\": \"String\",
                \"userinfo.token.claim\": \"false\"
            }
        }"
    echo "Mappers added."
fi

# Create roles
for ROLE in user couple admin; do
    echo "Checking role '$ROLE'..."
    if ! curl -sf -H "$(auth)" "$KEYCLOAK_URL/admin/realms/$REALM/roles/$ROLE" > /dev/null 2>&1; then
        echo "Creating role '$ROLE'..."
        curl -sf -X POST "$KEYCLOAK_URL/admin/realms/$REALM/roles" \
            -H "$(auth)" -H "Content-Type: application/json" \
            -d "{\"name\": \"$ROLE\"}"
    fi
done
echo "Roles ready."

# Create test user
echo "Checking test user..."
TEST_USER_RESP=$(curl -sf -H "$(auth)" "$KEYCLOAK_URL/admin/realms/$REALM/users?username=testuser")
if echo "$TEST_USER_RESP" | grep -q '"username"'; then
    echo "Test user already exists."
else
    echo "Creating test user..."
    curl -sf -X POST "$KEYCLOAK_URL/admin/realms/$REALM/users" \
        -H "$(auth)" -H "Content-Type: application/json" \
        -d "{
            \"username\": \"testuser\",
            \"email\": \"test@wednest.com\",
            \"firstName\": \"Test\",
            \"lastName\": \"User\",
            \"enabled\": true,
            \"emailVerified\": true,
            \"credentials\": [{\"type\": \"password\", \"value\": \"Test@1234\", \"temporary\": false}]
        }"

    # Assign user role
    USER_ID=$(curl -sf -H "$(auth)" "$KEYCLOAK_URL/admin/realms/$REALM/users?username=testuser" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
    ROLE_ID=$(curl -sf -H "$(auth)" "$KEYCLOAK_URL/admin/realms/$REALM/roles/user" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
    ROLE_NAME=$(curl -sf -H "$(auth)" "$KEYCLOAK_URL/admin/realms/$REALM/roles/user" | grep -o '"name":"[^"]*"' | head -1 | cut -d'"' -f4)

    curl -sf -X POST "$KEYCLOAK_URL/admin/realms/$REALM/users/$USER_ID/role-mappings/realm" \
        -H "$(auth)" -H "Content-Type: application/json" \
        -d "[{\"id\": \"$ROLE_ID\", \"name\": \"$ROLE_NAME\"}]"
    echo "Test user created with 'user' role."
fi

echo ""
echo "=== Setup Complete ==="
echo "Keycloak:  $KEYCLOAK_URL"
echo "Admin:     $ADMIN_USER / $ADMIN_PASS"
echo "Client:    $CLIENT_ID / $CLIENT_SECRET"
echo "Test user: testuser / Test@1234"
echo ""
