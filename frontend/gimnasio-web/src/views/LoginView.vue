<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()
const mode = ref<'login' | 'register'>('login')
const form = reactive({ firstName: '', lastName: '', email: '', password: '', rememberMe: false })

async function submit() {
  try {
    if (mode.value === 'login') {
      await auth.login({ email: form.email, password: form.password, rememberMe: form.rememberMe })
    } else {
      await auth.register({
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        password: form.password,
      })
    }
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/'
    await router.push(redirect)
  } catch {
    // El store expone el mensaje para la interfaz.
  }
}
</script>

<template>
  <main class="auth-page">
    <section class="auth-story">
      <span class="brand-mark large">A</span>
      <span class="eyebrow light">Proyecto Atlas</span>
      <h1>Gestión y entrenamiento, finalmente conectados.</h1>
      <p>Tu rendimiento, recuperación y progreso físico en una sola plataforma personal.</p>
    </section>

    <section class="auth-panel">
      <div class="auth-card">
        <span class="eyebrow">Acceso seguro</span>
        <h2>{{ mode === 'login' ? 'Bienvenido de nuevo' : 'Crear cuenta inicial' }}</h2>
        <p v-if="mode === 'register'" class="form-hint">La primera cuenta creada recibirá el rol Administrador.</p>

        <div class="auth-tabs">
          <button type="button" :class="{ active: mode === 'login' }" @click="mode = 'login'">Ingresar</button>
          <button type="button" :class="{ active: mode === 'register' }" @click="mode = 'register'">Registrarme</button>
        </div>

        <form @submit.prevent="submit">
          <div v-if="mode === 'register'" class="form-row">
            <label>Nombre<input v-model.trim="form.firstName" required maxlength="80" /></label>
            <label>Apellido<input v-model.trim="form.lastName" required maxlength="80" /></label>
          </div>
          <label>Correo electrónico<input v-model.trim="form.email" type="email" required autocomplete="email" /></label>
          <label>Contraseña<input v-model="form.password" type="password" required minlength="8" autocomplete="current-password" /></label>
          <label v-if="mode === 'login'" class="checkbox-label">
            <input v-model="form.rememberMe" type="checkbox" /> Mantener mi sesión
          </label>
          <div v-if="auth.error" class="notice error">{{ auth.error }}</div>
          <button class="primary-button full" type="submit" :disabled="auth.loading">
            {{ auth.loading ? 'Procesando…' : mode === 'login' ? 'Ingresar' : 'Crear cuenta' }}
          </button>
        </form>
      </div>
    </section>
  </main>
</template>
