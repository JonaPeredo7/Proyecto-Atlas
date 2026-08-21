<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from './stores/auth'
import { pendingFor, syncPending } from './services/offlineQueue'
import { getAtlasOverview } from './services/atlasService'
import type { AtlasOverview } from './types/atlas'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
interface InstallPromptEvent extends Event { prompt: () => Promise<void>; userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }> }
const installPrompt = ref<InstallPromptEvent | null>(null)
const mobileMenuOpen = ref(false)
const online = ref(navigator.onLine)
const installed = window.matchMedia('(display-mode: standalone)').matches
const pendingCount = ref(0)
const syncing = ref(false)
const reminderOpen = ref(false)
const reminderLoading = ref(false)
const reminderOverview = ref<AtlasOverview | null>(null)
const notificationSupported = 'Notification' in window
const notificationEnabled = ref(notificationSupported && Notification.permission === 'granted' && localStorage.getItem('atlas-device-reminders') === 'enabled')
const notificationMessage = ref('')
const usesPublicLayout = computed(() => Boolean(route.meta.publicLayout))
const reminders = computed(() => reminderOverview.value?.hub.actions.filter(item => item.state === 'pending' || item.state === 'active' || item.state === 'attention') ?? [])
const initials = computed(() => {
  if (!auth.user) return 'US'
  return `${auth.user.firstName[0] ?? ''}${auth.user.lastName[0] ?? ''}`.toUpperCase()
})

async function logout() {
  await auth.logout()
  await router.push({ name: 'login' })
}
function captureInstall(event: Event) { event.preventDefault(); installPrompt.value = event as InstallPromptEvent }
function updateConnection() { online.value = navigator.onLine; if (online.value) void synchronize() }
async function refreshPending() { pendingCount.value = auth.user ? (await pendingFor(auth.user.id)).length : 0 }
async function refreshReminders() {
  if (!auth.user || !navigator.onLine || reminderLoading.value) return
  reminderLoading.value = true
  try { reminderOverview.value = await getAtlasOverview(); notifyDailySummary() }
  catch { reminderOverview.value = null }
  finally { reminderLoading.value = false }
}
async function toggleReminders() { reminderOpen.value = !reminderOpen.value; if (reminderOpen.value) await refreshReminders() }
async function toggleDeviceReminders() {
  notificationMessage.value = ''
  if (!notificationSupported) { notificationMessage.value = 'Este navegador no admite avisos del dispositivo.'; return }
  if (notificationEnabled.value) {
    notificationEnabled.value = false
    localStorage.removeItem('atlas-device-reminders')
    notificationMessage.value = 'Avisos desactivados en este dispositivo.'
    return
  }
  const permission = Notification.permission === 'granted' ? 'granted' : await Notification.requestPermission()
  if (permission !== 'granted') { notificationMessage.value = 'El navegador no concedió permiso. Podés cambiarlo desde la configuración del sitio.'; return }
  notificationEnabled.value = true
  localStorage.setItem('atlas-device-reminders', 'enabled')
  notificationMessage.value = 'Avisos activados. Atlas mostrará como máximo un resumen diario.'
  notifyDailySummary(true)
}
function notifyDailySummary(force=false) {
  if (!notificationEnabled.value || Notification.permission !== 'granted' || !auth.user || !reminders.value.length) return
  const date = new Date().toLocaleDateString('en-CA')
  const key = `atlas-reminder-sent:${auth.user.id}:${date}`
  if (!force && localStorage.getItem(key)) return
  const notification = new Notification('Proyecto Atlas', { body: `Tenés ${reminders.value.length} ${reminders.value.length===1?'recordatorio pendiente':'recordatorios pendientes'}.`, tag: `atlas-daily-${date}`, silent: true })
  localStorage.setItem(key, new Date().toISOString())
  notification.onclick = () => { window.focus(); void router.push(reminders.value[0]?.route ?? '/'); notification.close() }
}
async function synchronize() { if (!auth.user || !navigator.onLine || syncing.value) return; syncing.value = true; try { await syncPending(auth.user.id); await refreshPending() } finally { syncing.value = false } }
async function installAtlas() {
  if (!installPrompt.value) return
  await installPrompt.value.prompt()
  const choice = await installPrompt.value.userChoice
  if (choice.outcome === 'accepted') installPrompt.value = null
}
onMounted(() => {
  window.addEventListener('beforeinstallprompt', captureInstall)
  window.addEventListener('online', updateConnection)
  window.addEventListener('offline', updateConnection)
  window.addEventListener('atlas-queue-change', refreshPending)
  void refreshPending()
  if (auth.user) void refreshReminders()
})
onUnmounted(() => {
  window.removeEventListener('beforeinstallprompt', captureInstall)
  window.removeEventListener('online', updateConnection)
  window.removeEventListener('offline', updateConnection)
  window.removeEventListener('atlas-queue-change', refreshPending)
})
watch(() => auth.user?.id, () => { void refreshPending(); if (auth.user && navigator.onLine) { void synchronize(); void refreshReminders() } })
watch(() => route.fullPath, () => { reminderOpen.value = false; if (auth.user && navigator.onLine) void refreshReminders() })
</script>

<template>
  <RouterView v-if="usesPublicLayout" />

  <div v-else class="app-shell">
    <aside class="sidebar">
      <div class="brand">
        <span class="brand-mark">A</span>
        <div>
          <strong>Proyecto Atlas</strong>
          <small>Rendimiento personal</small>
        </div>
      </div>

      <nav aria-label="Navegación principal">
        <RouterLink to="/">Hoy</RouterLink>
        <RouterLink to="/mi-perfil">Mi perfil</RouterLink>
        <RouterLink to="/entrenamiento">Entrenamiento</RouterLink>
        <RouterLink to="/plan">Plan maestro</RouterLink>
        <RouterLink to="/bitacora">Bitácora</RouterLink>
        <RouterLink to="/evaluacion">Evaluación</RouterLink>
        <RouterLink to="/informe">Informe</RouterLink>
        <RouterLink to="/agenda">Agenda</RouterLink>
        <RouterLink to="/rodilla">Salud y rodilla</RouterLink>
        <RouterLink to="/resumen-semanal">Resumen semanal</RouterLink>
        <RouterLink to="/respuesta-24h">Respuesta 24 h</RouterLink>
        <RouterLink to="/tendencias">Tendencias</RouterLink>
        <RouterLink to="/mediciones">Mediciones</RouterLink>
        <RouterLink to="/mis-datos">Mis datos</RouterLink>
      </nav>

      <div class="sidebar-footer">
        <span class="status-dot"></span>
        Núcleo Atlas activo
      </div>
    </aside>

    <main class="main-content">
      <header class="topbar">
        <div>
          <span class="eyebrow">Proyecto Atlas</span>
          <strong>Seguimiento personal basado en evidencia</strong>
        </div>
        <div class="topbar-actions">
          <button class="reminder-button" type="button" :class="{active:reminderOpen}" :aria-expanded="reminderOpen" aria-label="Abrir recordatorios" @click="toggleReminders"><span>!</span><b v-if="reminders.length">{{reminders.length}}</b><small>Recordatorios</small></button>
          <button class="profile-chip" type="button" title="Cerrar sesión" @click="logout"><span>{{ initials }}</span>{{ auth.user?.firstName ?? 'Usuario' }} · Salir</button>
        </div>
      </header>

      <div v-if="reminderOpen" class="reminder-backdrop" @click.self="reminderOpen=false">
        <aside class="reminder-center">
          <header><div><span class="eyebrow">Centro de recordatorios</span><h2>Qué requiere tu atención</h2></div><button class="icon-button" aria-label="Cerrar" @click="reminderOpen=false">×</button></header>
          <p class="reminder-intro">Sólo aparecen tareas pendientes, en curso o que necesitan revisión. Atlas no convierte sugerencias opcionales en alertas.</p>
          <div v-if="reminderLoading" class="reminder-empty">Actualizando recordatorios…</div>
          <div v-else-if="reminders.length" class="reminder-list">
            <RouterLink v-for="item in reminders" :key="`${item.kind}-${item.title}`" :to="item.route" :class="item.state" @click="reminderOpen=false"><span>{{item.state==='attention'?'!':item.state==='active'?'→':'•'}}</span><div><strong>{{item.title}}</strong><small>{{item.detail}}</small></div><em>{{item.state==='attention'?'Revisar':item.state==='active'?'En curso':'Pendiente'}}</em></RouterLink>
          </div>
          <div v-else class="reminder-empty"><span>✓</span><strong>No hay pendientes esenciales</strong><p>Las sugerencias opcionales permanecen disponibles en Hoy.</p></div>
          <section class="device-reminders"><div><strong>Avisos del dispositivo</strong><small>Mensaje genérico, sin datos de salud. Máximo un resumen diario mientras Atlas esté activo.</small></div><button class="secondary-button" :class="{enabled:notificationEnabled}" @click="toggleDeviceReminders">{{notificationEnabled?'Desactivar':'Activar'}}</button><p v-if="notificationMessage">{{notificationMessage}}</p></section>
          <footer><RouterLink to="/agenda" @click="reminderOpen=false">Abrir agenda completa →</RouterLink><button class="text-button" :disabled="reminderLoading" @click="refreshReminders">Actualizar</button></footer>
        </aside>
      </div>

      <div v-if="!online" class="offline-banner"><strong>Sin conexión</strong><span>El check-in y el cierre de entrenamiento pueden quedar pendientes en este dispositivo.</span></div>
      <div v-if="pendingCount" class="sync-banner"><div><strong>{{pendingCount}} {{pendingCount===1?'registro pendiente':'registros pendientes'}}</strong><span>{{online?'Listos para sincronizar con tu cuenta.':'Se enviarán al recuperar conexión.'}}</span></div><button class="secondary-button" :disabled="!online||syncing" @click="synchronize">{{syncing?'Sincronizando…':'Sincronizar ahora'}}</button></div>
      <div v-if="installPrompt && !installed" class="install-banner"><div><strong>Instalar Proyecto Atlas</strong><span>Acceso rápido y experiencia de aplicación en este dispositivo.</span></div><button class="primary-button" @click="installAtlas">Instalar</button><button class="text-button" aria-label="Ocultar sugerencia" @click="installPrompt=null">Ahora no</button></div>

      <RouterView />
    </main>

    <nav class="mobile-dock" aria-label="Navegación móvil">
      <RouterLink to="/" @click="mobileMenuOpen=false"><span>⌂</span>Hoy</RouterLink>
      <RouterLink to="/entrenamiento" @click="mobileMenuOpen=false"><span>＋</span>Entrenar</RouterLink>
      <RouterLink to="/agenda" @click="mobileMenuOpen=false"><span>□</span>Agenda</RouterLink>
      <RouterLink to="/rodilla" @click="mobileMenuOpen=false"><span>◇</span>Salud</RouterLink>
      <button :class="{active:mobileMenuOpen}" @click="mobileMenuOpen=!mobileMenuOpen"><span>•••</span>Más</button>
    </nav>

    <div v-if="mobileMenuOpen" class="mobile-menu-backdrop" @click.self="mobileMenuOpen=false">
      <section class="mobile-more-menu"><header><div><span class="eyebrow">Proyecto Atlas</span><h2>Todos los módulos</h2></div><button class="text-button" @click="mobileMenuOpen=false">Cerrar</button></header><nav>
        <RouterLink to="/mi-perfil" @click="mobileMenuOpen=false">Mi perfil</RouterLink><RouterLink to="/plan" @click="mobileMenuOpen=false">Plan maestro</RouterLink><RouterLink to="/bitacora" @click="mobileMenuOpen=false">Bitácora</RouterLink><RouterLink to="/evaluacion" @click="mobileMenuOpen=false">Evaluación</RouterLink><RouterLink to="/informe" @click="mobileMenuOpen=false">Informe</RouterLink><RouterLink to="/resumen-semanal" @click="mobileMenuOpen=false">Resumen semanal</RouterLink><RouterLink to="/respuesta-24h" @click="mobileMenuOpen=false">Respuesta 24 h</RouterLink><RouterLink to="/tendencias" @click="mobileMenuOpen=false">Tendencias</RouterLink><RouterLink to="/mediciones" @click="mobileMenuOpen=false">Mediciones</RouterLink><RouterLink to="/mis-datos" @click="mobileMenuOpen=false">Mis datos</RouterLink>
      </nav></section>
    </div>
  </div>
</template>
