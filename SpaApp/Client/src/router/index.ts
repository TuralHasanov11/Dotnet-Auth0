import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import { useAuthenticationStore } from '@/stores/authentication'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/courses',
      name: 'courses',
      children: [
        {
          path: '',
          name: 'courses-index',
          component: () => import('../views/Courses/IndexView.vue'),
          meta: { requiresPermission: 'view:course' }
        }
      ]
    },
    {
      path: '/authentication',
      name: 'authentication',
      children: [
        {
          path: 'profile',
          name: 'authentication-profile',
          component: () => import('../views/Authentication/ProfileView.vue')
        }
      ]
    }
  ]
})

router.beforeResolve(async (to, from, next) => {
  const authenticationStore = useAuthenticationStore()

  if (
    to.meta.requiresPermission &&
    !authenticationStore.hasPermission(to.meta.requiresPermission as string)
  ) {
    return next({ name: 'home' })
  } else if (to.meta.requiresAuth && !authenticationStore.isAuthenticated) {
    return next({ name: 'authentication-login', query: { redirect: to.fullPath } })
  } else if (to.meta.requiresGuest && authenticationStore.isAuthenticated) {
    return next({ name: 'home' })
  } else {
    return next()
  }
})

export default router
