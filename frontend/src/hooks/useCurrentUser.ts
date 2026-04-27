import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'

export const CURRENT_USER_KEY = ['auth', 'me']

export function useCurrentUser() {
  return useQuery({
    queryKey: CURRENT_USER_KEY,
    queryFn: api.auth.me,
    retry: false,
    staleTime: 5 * 60 * 1000,
  })
}
