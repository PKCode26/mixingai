export interface AuthUser {
  userId: string
  username: string
  email: string
  firstName: string
  lastName: string
  isAdmin: boolean
}

export interface LoginResponse {
  token: string
  expiresAtUtc: string
  user: AuthUser
}

export interface LoginRequest {
  usernameOrEmail: string
  password: string
}
