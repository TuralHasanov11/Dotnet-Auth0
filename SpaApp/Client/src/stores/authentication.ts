import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { User } from '@/models'
import { Result, type DefaultResult } from '@/primitives/result'
import { useHttpClient } from '@/composables/useHttpClient'
import { AppError, type ResponseError } from '@/primitives/error'

const nullUser: User = {
  email: '',
  name: '',
  roles: [],
  permissions: []
}

export const useAuthenticationStore = defineStore('authentication', () => {
  const user = ref<User>()
  const isAuthenticated = computed(() => !isLoading.value && user?.value?.name !== nullUser.name)
  const isLoading = computed(() => user?.value === undefined)

  function hasPermission(permission: string): boolean {
    return (isAuthenticated.value && user.value?.permissions?.includes(permission)) ?? false
  }

  function register(): void {
    window.location.href = '/api/authentication/register'
  }

  function login(): void {
    window.location.href = '/api/authentication/login'
  }

  function logout(): void {
    window.location.href = '/api/authentication/logout'
  }

  async function getUserInfo(): Promise<DefaultResult> {
    try {
      const { data } = await useHttpClient().get<User>('api/authentication/user-info', {
        headers: {
          'X-Requested-With': 'XMLHttpRequest'
        }
      })

      user.value = data
    } catch (error: any) {
      user.value = nullUser
      const apiError = error as ResponseError
      return Result.failure(AppError.failure(apiError.message))
    }

    return Result.success()
  }

  return {
    user,
    isAuthenticated,
    register,
    login,
    logout,
    getUserInfo,
    hasPermission,
    isLoading
  }
})
