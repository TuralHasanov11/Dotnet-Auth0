import type { AxiosInstance } from 'axios'
import { httpClient } from '@/utils/httpClient'

export function useHttpClient(): AxiosInstance {
  return httpClient
}
