import type { AppError } from './error'

export type DefaultResult = Result<undefined>

export class Result<T> {
  public readonly value?: T
  public readonly isSuccess: boolean
  public readonly error?: AppError

  private constructor(value?: T, error?: AppError) {
    if (error) {
      this.error = error
      this.isSuccess = false
    } else {
      this.value = value
      this.isSuccess = true
    }
  }

  public static successWithValue<T>(value: T): Result<T> {
    return new Result<T>(value)
  }

  public static success(): Result<undefined> {
    return new Result<undefined>()
  }

  public static failure(error: AppError): Result<undefined> {
    return new Result<undefined>(undefined, error)
  }

  public static failureWithValue<T>(error: AppError): Result<T> {
    return new Result<T>(undefined, error)
  }

  public static create(): Result<undefined> {
    return new Result<undefined>()
  }

  public static createWithValue<T>(value: T): Result<T> {
    return new Result<T>(value)
  }

  public get isFailure(): boolean {
    return !this.isSuccess
  }
}
