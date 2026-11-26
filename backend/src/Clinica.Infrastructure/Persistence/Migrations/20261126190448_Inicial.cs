using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "clinica");

            migrationBuilder.EnsureSchema(
                name: "identidade");

            migrationBuilder.CreateTable(
                name: "auditoria",
                schema: "clinica",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ocorrido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    perfil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    acao = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    entidade = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    paciente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dados_antes = table.Column<string>(type: "jsonb", nullable: true),
                    dados_depois = table.Column<string>(type: "jsonb", nullable: true),
                    ip = table.Column<IPAddress>(type: "inet", nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria", x => x.id);
                    table.CheckConstraint("ck_auditoria_acao", "acao in ('Leitura','Criacao','Alteracao','Estorno','Login','LoginFalho')");
                });

            migrationBuilder.CreateTable(
                name: "chaves_protecao_dados",
                schema: "identidade",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(type: "text", nullable: true),
                    xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chaves_protecao_dados", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parametros",
                schema: "clinica",
                columns: table => new
                {
                    chave = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    valor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametros", x => x.chave);
                });

            migrationBuilder.CreateTable(
                name: "perfis",
                schema: "identidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perfis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "identidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    troca_senha_obrigatoria = table.Column<bool>(type: "boolean", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "perfis_claims",
                schema: "identidade",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perfis_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_perfis_claims_perfis_role_id",
                        column: x => x.role_id,
                        principalSchema: "identidade",
                        principalTable: "perfis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessoes",
                schema: "identidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket = table.Column<byte[]>(type: "bytea", nullable: false),
                    criada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expira_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expira_absoluto_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revogada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_sessoes_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalSchema: "identidade",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_claims",
                schema: "identidade",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuarios_claims_usuarios_user_id",
                        column: x => x.user_id,
                        principalSchema: "identidade",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_logins",
                schema: "identidade",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "FK_usuarios_logins_usuarios_user_id",
                        column: x => x.user_id,
                        principalSchema: "identidade",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_perfis",
                schema: "identidade",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_perfis", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_usuarios_perfis_perfis_role_id",
                        column: x => x.role_id,
                        principalSchema: "identidade",
                        principalTable: "perfis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuarios_perfis_usuarios_user_id",
                        column: x => x.user_id,
                        principalSchema: "identidade",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_tokens",
                schema: "identidade",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "FK_usuarios_tokens_usuarios_user_id",
                        column: x => x.user_id,
                        principalSchema: "identidade",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "clinica",
                table: "parametros",
                columns: new[] { "chave", "valor" },
                values: new object[,]
                {
                    { "antecedencia_cancelamento_horas", "24" },
                    { "desconto_dinheiro_percentual", "10" },
                    { "janela_agenda_meses", "2" }
                });

            migrationBuilder.InsertData(
                schema: "identidade",
                table: "perfis",
                columns: new[] { "id", "concurrency_stamp", "name", "normalized_name" },
                values: new object[,]
                {
                    { new Guid("0196a000-0000-7000-8000-000000000001"), "0196a000-0000-7000-8000-000000000001", "Administrador", "ADMINISTRADOR" },
                    { new Guid("0196a000-0000-7000-8000-000000000002"), "0196a000-0000-7000-8000-000000000002", "Medico", "MEDICO" },
                    { new Guid("0196a000-0000-7000-8000-000000000003"), "0196a000-0000-7000-8000-000000000003", "Recepcao", "RECEPCAO" },
                    { new Guid("0196a000-0000-7000-8000-000000000004"), "0196a000-0000-7000-8000-000000000004", "Paciente", "PACIENTE" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_entidade_entidade_id",
                schema: "clinica",
                table: "auditoria",
                columns: new[] { "entidade", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_paciente_id_ocorrido_em",
                schema: "clinica",
                table: "auditoria",
                columns: new[] { "paciente_id", "ocorrido_em" });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_usuario_id_ocorrido_em",
                schema: "clinica",
                table: "auditoria",
                columns: new[] { "usuario_id", "ocorrido_em" });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "identidade",
                table: "perfis",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_perfis_claims_role_id",
                schema: "identidade",
                table: "perfis_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_sessoes_expira_em",
                schema: "identidade",
                table: "sessoes",
                column: "expira_em");

            migrationBuilder.CreateIndex(
                name: "IX_sessoes_usuario_id",
                schema: "identidade",
                table: "sessoes",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "identidade",
                table: "usuarios",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "identidade",
                table: "usuarios",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_claims_user_id",
                schema: "identidade",
                table: "usuarios_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_logins_user_id",
                schema: "identidade",
                table: "usuarios_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_perfis_role_id",
                schema: "identidade",
                table: "usuarios_perfis",
                column: "role_id");

            // Papel de grupo da aplicação: CRUD nas tabelas, mas auditoria é somente inserção (RNF-03).
            // O login de conexão é criado fora das migrations (docker/postgres/01-papel-aplicacao.sh) e herda este papel.
            migrationBuilder.Sql(@"
DO $$ BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'clinica_app') THEN CREATE ROLE clinica_app NOLOGIN; END IF;
END $$;
GRANT USAGE ON SCHEMA clinica, identidade TO clinica_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA clinica, identidade TO clinica_app;
REVOKE UPDATE, DELETE, TRUNCATE ON clinica.auditoria FROM clinica_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA clinica, identidade GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO clinica_app;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditoria",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "chaves_protecao_dados",
                schema: "identidade");

            migrationBuilder.DropTable(
                name: "parametros",
                schema: "clinica");

            migrationBuilder.DropTable(
                name: "perfis_claims",
                schema: "identidade");

            migrationBuilder.DropTable(
                name: "sessoes",
                schema: "identidade");

            migrationBuilder.DropTable(
                name: "usuarios_claims",
                schema: "identidade");

            migrationBuilder.DropTable(
                name: "usuarios_logins",
                schema: "identidade");

            migrationBuilder.DropTable(
                name: "usuarios_perfis",
                schema: "identidade");

            migrationBuilder.DropTable(
                name: "usuarios_tokens",
                schema: "identidade");

            migrationBuilder.DropTable(
                name: "perfis",
                schema: "identidade");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "identidade");
        }
    }
}
