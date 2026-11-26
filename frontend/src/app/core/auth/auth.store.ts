import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

/** Usuário da sessão (API-01 e API-02). */
export interface UsuarioSessao {
  id: string;
  nome: string;
  perfis: string[];
  pacienteId: string | null;
  medicoId: string | null;
  cadastroPendente: boolean;
  trocaSenhaObrigatoria: boolean;
}

/** Estado de autenticação. Nenhum token fica no JavaScript: a sessão vive em cookie HttpOnly (DT-45). */
@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly http = inject(HttpClient);
  private readonly _usuario = signal<UsuarioSessao | null>(null);

  readonly usuario = this._usuario.asReadonly();
  readonly autenticado = computed(() => this._usuario() !== null);

  /** Restaura a sessão (API-02); também faz o servidor emitir o cookie XSRF. */
  async restaurar(): Promise<void> {
    try {
      const r = await firstValueFrom(
        this.http.get<UsuarioSessao | null>('/api/v1/auth/sessao', { observe: 'response' }),
      );
      this._usuario.set(r.status === 200 ? r.body : null);
    } catch {
      this._usuario.set(null);
    }
  }

  /** Login (API-01). Lança `ErroApi` em caso de falha. */
  async entrar(email: string, senha: string): Promise<UsuarioSessao> {
    const u = await firstValueFrom(this.http.post<UsuarioSessao>('/api/v1/auth/login', { email, senha }));
    this._usuario.set(u);
    return u;
  }

  /** Logout (API-03). */
  async sair(): Promise<void> {
    try {
      await firstValueFrom(this.http.post<void>('/api/v1/auth/logout', {}));
    } finally {
      this._usuario.set(null);
    }
  }

  /** Esquece o usuário local (por exemplo, após 401). */
  limpar(): void {
    this._usuario.set(null);
  }
}
