#!/usr/bin/env bash
set -eu

# --- Check admin privileges ---
if [[ $EUID -ne 0 ]]; then
  echo "❌ This script must be run with administrator privileges (sudo)."
  echo "Please rerun it using: sudo $0"
  exit 1
fi

# --- User input ---
read -rp "Enter the path to the folder where the certificate will be stored: " cert_dir
read -rp "Enter the certificate name (e.g., localhost): " domain

org="localhost-ca"

# --- Prepare directories ---
mkdir -p "$cert_dir"
chown "$SUDO_USER":"users" "$cert_dir"

# --- Generate CA (root certificate) ---
openssl genpkey -algorithm RSA -out "$cert_dir/ca.key"
openssl req -x509 -key "$cert_dir/ca.key" \
  -out "$cert_dir/ca.crt" \
  -subj "/CN=$org/O=$org"

# --- Generate certificate ---
openssl genpkey -algorithm RSA -out "$cert_dir/$domain.key"
openssl req -new -key "$cert_dir/$domain.key" \
  -out "$cert_dir/$domain.csr" \
  -subj "/CN=$domain/O=$org"

openssl x509 -req \
  -in "$cert_dir/$domain.csr" -days 365 \
  -out "$cert_dir/$domain.crt" \
  -CA "$cert_dir/ca.crt" \
  -CAkey "$cert_dir/ca.key" \
  -CAcreateserial \
  -extfile <(cat <<END
basicConstraints = CA:FALSE
subjectKeyIdentifier = hash
authorityKeyIdentifier = keyid,issuer
subjectAltName = DNS:$domain
END
)

# --- Create PKCS12 (.pfx)
openssl pkcs12 -export -out "$cert_dir/$domain.pfx" \
  -inkey "$cert_dir/$domain.key" \
  -in "$cert_dir/$domain.crt"

# --- Add trusted CA ---
trust anchor "$cert_dir/ca.crt"

echo "✅ Certificates have been successfully created in: $cert_dir"