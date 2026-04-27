import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api'
import { CURRENT_USER_KEY } from '../hooks/useCurrentUser'
import styles from './LoginPage.module.css'

export default function LoginPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [usernameOrEmail, setUsernameOrEmail] = useState('')
  const [password, setPassword] = useState('')

  const login = useMutation({
    mutationFn: api.auth.login,
    onSuccess: (data) => {
      queryClient.setQueryData(CURRENT_USER_KEY, data.user)
      navigate('/')
    },
  })

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    login.mutate({ usernameOrEmail, password })
  }

  return (
    <div className={styles.page}>
      <div className={styles.card}>
        <h1 className={styles.title}>MixingAI</h1>
        <p className={styles.subtitle}>Anmelden</p>
        <form onSubmit={handleSubmit} className={styles.form}>
          <label className={styles.label}>
            Benutzername oder E-Mail
            <input
              className={styles.input}
              type="text"
              autoComplete="username"
              value={usernameOrEmail}
              onChange={(e) => setUsernameOrEmail(e.target.value)}
              required
            />
          </label>
          <label className={styles.label}>
            Passwort
            <input
              className={styles.input}
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </label>
          {login.isError && (
            <p className={styles.error}>
              Anmeldung fehlgeschlagen. Benutzername oder Passwort falsch.
            </p>
          )}
          <button className={styles.submit} type="submit" disabled={login.isPending}>
            {login.isPending ? 'Anmelden…' : 'Anmelden'}
          </button>
        </form>
      </div>
    </div>
  )
}
