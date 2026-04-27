import { Navigate } from 'react-router-dom'
import { useCurrentUser } from '../hooks/useCurrentUser'

interface Props {
  children: React.ReactNode
  requireAdmin?: boolean
}

export default function ProtectedRoute({ children, requireAdmin = false }: Props) {
  const { data: user, isLoading, isError } = useCurrentUser()

  if (isLoading) {
    return <div style={{ padding: '2rem', color: '#6b7280' }}>Laden…</div>
  }

  if (isError || !user) {
    return <Navigate to="/login" replace />
  }

  if (requireAdmin && !user.isAdmin) {
    return <Navigate to="/" replace />
  }

  return <>{children}</>
}
