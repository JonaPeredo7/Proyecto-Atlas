import { defineStore } from 'pinia'
import * as authService from '../services/authService'
import type { AuthUser, LoginRequest, RegisterRequest } from '../types/auth'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as AuthUser | null,
    checked: false,
    loading: false,
    error: null as string | null,
  }),
  actions: {
    async ensureSession() {
      if (this.checked) return
      this.user = await authService.getCurrentUser().catch(() => null)
      this.checked = true
    },
    async login(request: LoginRequest) {
      await this.run(() => authService.login(request))
    },
    async register(request: RegisterRequest) {
      await this.run(() => authService.register(request))
    },
    async logout() {
      await authService.logout()
      this.user = null
      this.checked = true
    },
    async run(action: () => Promise<AuthUser>) {
      this.loading = true
      this.error = null
      try {
        this.user = await action()
        this.checked = true
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Ocurrió un error inesperado.'
        throw error
      } finally {
        this.loading = false
      }
    },
  },
})
