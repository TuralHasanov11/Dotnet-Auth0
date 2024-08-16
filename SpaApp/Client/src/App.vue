<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import { useAuthenticationStore } from '@/stores/authentication'
import { Permissions } from '@/models'

const authenticationStore = useAuthenticationStore()

function login() {
  authenticationStore.login()
}

function register() {
  authenticationStore.register()
}

function logout() {
  authenticationStore.logout()
}
</script>

<template>
  <header>
    <nav class="navbar navbar-expand-lg bg-body-tertiary" aria-label="Main navigation">
      <div class="container-fluid">
        <RouterLink class="navbar-brand" :to="{ name: 'home' }">Home</RouterLink>
        <button
          class="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#navbarNav"
          aria-controls="navbarNav"
          aria-expanded="false"
          aria-label="Toggle navigation"
        >
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
          <template v-if="authenticationStore.isAuthenticated">
            <ul class="navbar-nav">
              <li class="nav-item">
                <RouterLink
                  class="nav-link"
                  v-if="authenticationStore.hasPermission(Permissions.ViewCourse)"
                  :to="{ name: 'courses-index' }"
                  >Courses</RouterLink
                >
              </li>
            </ul>
            <ul class="navbar-nav ms-auto">
              <li class="nav-item">
                <RouterLink :to="{ name: 'authentication-profile' }" class="nav-link">{{
                  authenticationStore.user?.name
                }}</RouterLink>
              </li>
              <li class="nav-item">
                <a class="nav-link" href="javascript:void(0)" @click="logout">Logout</a>
              </li>
            </ul>
          </template>
          <template v-else>
            <ul class="navbar-nav ms-auto">
              <li class="nav-item">
                <a class="nav-link" href="javascript:void(0)" @click="login">Login</a>
              </li>
              <li class="nav-item">
                <a class="nav-link" href="javascript:void(0)" @click="register">Register</a>
              </li>
            </ul>
          </template>
        </div>
      </div>
    </nav>
  </header>

  <RouterView />
</template>
