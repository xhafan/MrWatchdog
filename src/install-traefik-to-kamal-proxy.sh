#!/bin/sh
# Alpine Linux Traefik installer which forwards requests to Kamal proxy on port 8080, and adds real client IP address to X-Forwarded-For and X-Forwarded-Proto headers
# Usage: ./install-traefik.sh --domain domain1.com --domain domain2.com --email user@example.com

set -ex

# Default values
DOMAINS=""
EMAIL=""

# Parse arguments
while [ "$#" -gt 0 ]; do
  case "$1" in
    --domain)
      shift
      DOMAINS="$DOMAINS $1"
      ;;
    --email)
      shift
      EMAIL="$1"
      ;;
    *)
      echo "Unknown parameter: $1"
      exit 1
      ;;
  esac
  shift
done

if [ -z "$DOMAINS" ] || [ -z "$EMAIL" ]; then
  echo "Usage: $0 --domain domain1.com [--domain domain2.com ...] --email user@example.com"
  exit 1
fi

# 1. Install required packages
apk update
apk add --no-cache curl tar bash ca-certificates

# 2. Download latest Traefik (v3)
TRAEFIK_VERSION=$(curl -s https://api.github.com/repos/traefik/traefik/releases/latest | grep '"tag_name":' | sed -E 's/.*"v([^"]+)".*/\1/')
curl -4 -L "https://github.com/traefik/traefik/releases/download/v$TRAEFIK_VERSION/traefik_v${TRAEFIK_VERSION}_linux_amd64.tar.gz" -o /tmp/traefik.tar.gz
tar xzf /tmp/traefik.tar.gz -C /tmp
mv /tmp/traefik /usr/local/bin/
chmod +x /usr/local/bin/traefik

# 3. Create directories
mkdir -p /etc/traefik/dynamic
touch /etc/traefik/acme.json
chmod 600 /etc/traefik/acme.json

# 4. Create main Traefik config
cat > /etc/traefik/traefik.yml <<EOF
entryPoints:
  web:
    address: ":80"
  websecure:
    address: ":443"

providers:
  file:
    directory: "/etc/traefik/dynamic"
    watch: true

api:
  dashboard: true

certificatesResolvers:
  myresolver:
    acme:
      email: "$EMAIL"
      storage: "/etc/traefik/acme.json"
      httpChallenge:
        entryPoint: web
EOF

# 5. Prepare domains string for Host rule
DOMAINS_RULE=""
for d in $DOMAINS; do
  if [ -z "$DOMAINS_RULE" ]; then
    DOMAINS_RULE="Host(\`$d\`)"
  else
    DOMAINS_RULE="$DOMAINS_RULE || Host(\`$d\`)"
  fi
done

# 6. Create dynamic config to forward to Kamal
cat > /etc/traefik/dynamic/kamal-forward.yml <<EOF
http:
  routers:
    to-kamal:
      entryPoints:
        - web
        - websecure
      rule: "$DOMAINS_RULE"
      service: kamal-service
      tls:
        certResolver: myresolver
      middlewares:
        - add-forwarded-headers

  services:
    kamal-service:
      loadBalancer:
        servers:
          - url: "http://127.0.0.1:8080"

  middlewares:
    add-forwarded-headers:
      headers:
        customRequestHeaders:
          X-Forwarded-For: "{remoteip}"
          X-Forwarded-Proto: "{scheme}"
EOF

# 7. Create OpenRC service for Alpine
cat > /etc/init.d/traefik <<'EOF'
#!/sbin/openrc-run

description="Traefik reverse proxy"

command="/usr/local/bin/traefik"
command_args="--configFile=/etc/traefik/traefik.yml"
command_background="yes"
pidfile="/var/run/traefik.pid"
name="traefik"

depend() {
    need net
}
EOF

chmod +x /etc/init.d/traefik

# 8. Enable and start Traefik via OpenRC
rc-service traefik stop || true
pkill -x traefik || true   # make sure any leftover Traefik is killed
rc-update add traefik default
rc-service traefik start

echo "Traefik installed and started."
echo "Forwarding traffic for: $DOMAINS to Kamal proxy on 127.0.0.1:8080"
