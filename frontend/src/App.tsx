import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import ProtectedRoute from './components/ProtectedRoute'
import AppShell from './components/AppShell'
import LoginPage from './pages/LoginPage'
import HomePage from './pages/HomePage'
import DocumentsPage from './pages/DocumentsPage'
import ImportReviewPage from './pages/ImportReviewPage'
import TrialsPage from './pages/TrialsPage'
import AdminPage from './pages/AdminPage'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          element={
            <ProtectedRoute>
              <AppShell />
            </ProtectedRoute>
          }
        >
          <Route index element={<HomePage />} />
          <Route path="dokumente/*" element={<DocumentsPage />} />
          <Route path="versuche/*" element={<TrialsPage />} />
          <Route
            path="admin/*"
            element={
              <ProtectedRoute requireAdmin>
                <AdminPage />
              </ProtectedRoute>
            }
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
        {/* Review-Seite außerhalb der Shell — braucht den vollen Viewport */}
        <Route
          path="/imports/:id/review"
          element={
            <ProtectedRoute>
              <ImportReviewPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </BrowserRouter>
  )
}
