import styles from './PlaceholderPage.module.css'

export default function AdminPage() {
  return (
    <div className={styles.page}>
      <h1 className={styles.heading}>Admin</h1>
      <p className={styles.hint}>Benutzerverwaltung und Systemeinstellungen — folgt in Phase 2.</p>
    </div>
  )
}
