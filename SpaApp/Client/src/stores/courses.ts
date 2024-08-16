import { useHttpClient } from '@/composables/useHttpClient'
import type { Course } from '@/models'
import { AppError, type ResponseError } from '@/primitives/error'
import { Result } from '@/primitives/result'
import { defineStore } from 'pinia'

export const useCourseStore = defineStore('course', () => {
  async function getCourses(): Promise<Result<Course[]>> {
    try {
      const { data } = await useHttpClient().get<Course[]>('api/courses')

      return Result.successWithValue<Course[]>(data)
    } catch (error: any) {
      const apiError = error as ResponseError
      return Result.failureWithValue<Course[]>(AppError.failure(apiError.message))
    }
  }

  return { getCourses }
})
