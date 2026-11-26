#!/bin/bash
# Cria o login da API como membro do papel de grupo clinica_app, que não tem UPDATE/DELETE em auditoria (RNF-03).
# O papel de grupo também é criado (se faltar) pela migration inicial.
set -euo pipefail
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<SQL
CREATE ROLE clinica_app NOLOGIN;
CREATE ROLE clinica_api LOGIN PASSWORD '${CLINICA_APP_SENHA}' IN ROLE clinica_app;
SQL
