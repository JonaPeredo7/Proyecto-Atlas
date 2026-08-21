export interface AuthUser {
  id: string
  email: string
  firstName: string
  lastName: string
  roles: string[]
}

export interface LoginRequest {
  email: string
  password: string
  rememberMe: boolean
}

export interface RegisterRequest {
  email: string
  password: string
  firstName: string
  lastName: string
}
