import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api'
import { CURRENT_USER_KEY, useCurrentUser } from '../hooks/useCurrentUser'
import styles from './AppShell.module.css'

export default function AppShell() {
  const { data: user } = useCurrentUser()
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const logout = useMutation({
    mutationFn: api.auth.logout,
    onSettled: () => {
      queryClient.setQueryData(CURRENT_USER_KEY, null)
      navigate('/login')
    },
  })

  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <NavLink to="/" className={styles.logo}>MixingAI</NavLink>
        <nav className={styles.nav}>
          <NavLink to="/dokumente" className={({ isActive }) => isActive ? `${styles.link} ${styles.active}` : styles.link}>
            Dokumente
          </NavLink>
          <NavLink to="/versuche" className={({ isActive }) => isActive ? `${styles.link} ${styles.active}` : styles.link}>
            Versuche &amp; Rezepte
          </NavLink>
          {user?.isAdmin && (
            <NavLink to="/admin" className={({ isActive }) => isActive ? `${styles.link} ${styles.active}` : styles.link}>
              Admin
            </NavLink>
          )}
        </nav>
        <div className={styles.user}>
          <span className={styles.username}>{user?.username}</span>
          <button className={styles.logoutBtn} onClick={() => logout.mutate()} disabled={logout.isPending}>
            Abmelden
          </button>
        </div>
      </header>
      <main className={styles.main}>
        <Outlet />
      </main>
    </div>
  )
}
