<script setup lang="ts">
import { useAuthenticationStore } from '@/stores/authentication'

const authenticationStore = useAuthenticationStore()
</script>
<template>
    <section id="courses" class="p-5">
        <h1>Profile</h1>

        <ul>
            <li>
                Name: <span>{{ authenticationStore.user?.name }}</span>
            </li>

            <li>
                Roles:
                <span v-for="role in authenticationStore.user?.roles" :key="role">{{ role }}</span>
            </li>

            <li>
                Permissions:
                <span class="p-1"
                      v-for="permission in authenticationStore.user?.permissions"
                      :key="permission">{{ permission }}</span>
            </li>
        </ul>
    </section>
</template>