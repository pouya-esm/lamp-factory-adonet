import { navSections } from '../components/navConfig.js'
import { useAuth } from '../context/AuthContext.jsx'

export function useNavSections() {
  const { hasAccess } = useAuth()

  const visibleNavSections = navSections
    .map((section) => ({
      ...section,
      items: section.items.filter((item) => {
        // Dashboard is always visible for authenticated users
        if (!item.resource) return true
        return hasAccess(item.resource)
      })
    }))
    .filter((section) => section.items.length > 0)

  return { visibleNavSections }
}
