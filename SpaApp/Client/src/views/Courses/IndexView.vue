<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useCourseStore } from '@/stores/courses'
import type { Course } from '@/models'

const courseStore = useCourseStore()

const courses = ref<Course[]>()

onMounted(async () => {
  const result = await courseStore.getCourses()

  if (result.isSuccess) {
    courses.value = result?.value
  }
})
</script>
<template>
  <section id="courses" class="p-5">
    <h1>Courses</h1>

    <ul>
      <li v-for="course in courses" :key="course.id">
        <span>{{ course.name }}</span>
      </li>
    </ul>
  </section>
</template>
