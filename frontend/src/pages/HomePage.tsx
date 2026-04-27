import { useNavigate } from 'react-router-dom'
import { useCurrentUser } from '../hooks/useCurrentUser'
import styles from './HomePage.module.css'

interface Tile {
  title: string
  description: string
  path: string
  adminOnly?: boolean
}

const TILES: Tile[] = [
  {
    title: 'Dokumente',
    description: 'Verwaltung und Erfassung / Import',
    path: '/dokumente',
  },
  {
    title: 'Versuche & Rezepte',
    description: 'Verwaltung, Suche und KI-Assistent',
    path: '/versuche',
  },
  {
    title: 'Admin',
    description: 'Benutzerverwaltung und Systemeinstellungen',
    path: '/admin',
    adminOnly: true,
  },
]

export default function HomePage() {
  const navigate = useNavigate()
  const { data: user } = useCurrentUser()

  const visibleTiles = TILES.filter((t) => !t.adminOnly || user?.isAdmin)

  return (
    <div>
      <h1 className={styles.heading}>Übersicht</h1>
      <div className={styles.grid}>
        {visibleTiles.map((tile) => (
          <button
            key={tile.path}
            className={styles.tile}
            onClick={() => navigate(tile.path)}
          >
            <span className={styles.tileTitle}>{tile.title}</span>
            <span className={styles.tileDesc}>{tile.description}</span>
          </button>
        ))}
      </div>
    </div>
  )
}
