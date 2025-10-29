# Simple Webserver with HTTPS Support

## Features

- Endpoints:
  - `/echo`
  - `/file`
  - `/video`
  - `/chat`
- Basic support for CSS and JavaScript
- Supports TLS 1.2 and 1.3

## Usage

1. Generate the certificate using the steps below.
2. In the code's main function, set the path to the `.pfx` certificate file.
3. When you start the server, you'll be prompted to enter the password you set for the `.pfx` file.
4. Test the server with:

curl -v https://localhost:4200/

## self sign Certificate

```bash
# 1. Generate private key
openssl genrsa -out localhost.key 2048

# 2. Create certificate signing request (CSR)
openssl req -new -key localhost.key -out localhost.csr
# Use 'localhost' as the Common Name (CN) when prompted

# 3. Create self-signed certificate
openssl x509 -req -days 365 -in localhost.csr -signkey localhost.key -out localhost.crt

# 4. Bundle certificate and key into .pfx file
openssl pkcs12 -export -out localhost.pfx -inkey localhost.key -in localhost.crt
# You will be prompted to set a password for the .pfx file

