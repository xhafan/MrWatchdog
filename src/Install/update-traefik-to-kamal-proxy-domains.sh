#!/bin/sh
# Usage: ./update-traefik-to-kamal-proxy-domains.sh --domain domain1.com --domain domain2.com

set -ex

# Default values
DOMAINS=""

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

if [ -z "$DOMAINS" ]; then
  echo "Usage: $0 --domain domain1.com [--domain domain2.com ...]"
  exit 1
fi

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

  services:
    kamal-service:
      loadBalancer:
        servers:
          - url: "http://127.0.0.1:8080"
EOF

# 8. Enable and start Traefik via OpenRC
rc-service traefik restart

echo "Traefik domains updated. Forwarding traffic for: $DOMAINS to Kamal proxy on 127.0.0.1:8080"
