import { HttpErrorResponse } from '@angular/common/http';

/** Erro da API já convertido de ProblemDetails (RFC 9457) para mensagens em pt-BR. */
export class ErroApi extends Error {
  constructor(
    /** Status HTTP (0 quando não houve resposta). */
    readonly status: number,
    /** Código de domínio, por exemplo `seguranca.antiforgery_invalido`. */
    readonly code: string | null,
    mensagem: string,
    /** Mensagens de validação por campo (nomes em camelCase). */
    readonly erros: Record<string, string[]> = {},
  ) {
    super(mensagem);
  }

  /** Converte a resposta HTTP de erro. */
  static de(e: HttpErrorResponse): ErroApi {
    const corpo = (e.error ?? {}) as Record<string, unknown>;
    const code = typeof corpo['code'] === 'string' ? corpo['code'] : null;
    const erros = (corpo['errors'] ?? {}) as Record<string, string[]>;
    return new ErroApi(e.status, code, ErroApi.mensagemPara(e.status, code), erros);
  }

  private static mensagemPara(status: number, code: string | null): string {
    if (code === 'auth.credenciais_invalidas') return 'E-mail ou senha incorretos.';
    if (code === 'seguranca.antiforgery_invalido') return 'Sua sessão de segurança expirou. Recarregue a página.';
    switch (status) {
      case 0:
        return 'Não foi possível conectar ao servidor. Verifique sua conexão.';
      case 400:
        return 'Há campos inválidos. Corrija e tente novamente.';
      case 401:
        return 'Sua sessão expirou. Entre novamente.';
      case 403:
        return 'Você não tem permissão para esta ação.';
      case 404:
        return 'Não encontrado.';
      case 409:
        return 'O registro foi alterado por outra pessoa. Recarregue e tente de novo.';
      case 422:
        return 'Não foi possível concluir a operação.';
      case 429:
        return 'Muitas tentativas. Aguarde um instante.';
      default:
        return 'Ocorreu um erro inesperado. Tente novamente.';
    }
  }
}
