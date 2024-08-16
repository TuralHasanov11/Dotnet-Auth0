export interface User {
  email: string
  name: string
  profileImage?: string
  roles: string[]
  permissions: string[]
}

export interface Role {
  id: string
  name: string
  permissions: string[]
}

export interface Course {
  id: string
  name: string
}

export class Permissions {
  public static readonly ViewCourse: string = 'view:course'

  public static all(): string[] {
    return [this.ViewCourse]
  }
}
