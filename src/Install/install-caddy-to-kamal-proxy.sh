#!/bin/sh
# Alpine Linux Caddy installer which forwards requests to Kamal proxy on port 8080, adds real client IP headers,
# and supports on-demand SSL certificates.
# Usage: ./install-caddy-to-kamal-proxy.sh --domains "domain1.com domain2.com"

set -ex

# Default values
DOMAINS=""

# Parse arguments
while [ "$#" -gt 0 ]; do
  case "$1" in
    --domains)
      shift
      DOMAINS="$1"   # take entire quoted string as one variable
      shift
      ;;
    *)
      echo "Unknown parameter: $1"
      exit 1
      ;;
  esac
done

if [ -z "$DOMAINS" ]; then
  echo "Usage: $0 --domains \"domain1.com domain2.com\""
  exit 1
fi

# 1. Install required packages
apk update
apk add --no-cache curl caddy tar bash ca-certificates

# 5. Generate Caddyfile
cat > /etc/caddy/Caddyfile <<EOF
{
    # Global options
}

$DOMAINS {
    reverse_proxy localhost:8080
    tls {
        on_demand
    }
}
EOF

# 6. Create OpenRC service
cat > /etc/init.d/caddy <<EOF
#!/sbin/openrc-run

name="Caddy"
description="Caddy web server"

command="caddy"
command_args="run --config /etc/caddy/Caddyfile --adapter caddyfile"
command_background="yes"
pidfile="/run/caddy.pid"
user="caddy"
group="caddy"

depend() {
    need net
    use logger dns
}

start_pre() {
    [ -d /run ] || mkdir -p /run
}
EOF

chmod +x /etc/init.d/caddy

# 7. Create Caddy user
adduser -D -g 'Caddy web server' caddy || true

# 8. Enable and start Caddy
rc-service caddy stop || true
pkill -x caddy || true
rc-update add caddy default
rc-service caddy start

echo "Caddy installed and started."
echo "Forwarding traffic for: $DOMAINS to Kamal proxy on 127.0.0.1:8080"
