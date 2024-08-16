import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import authentication from './plugins/authentication'

const app = createApp(App)

app.use(createPinia())
app.use(authentication)
app.use(router)

app.mount('#app')
