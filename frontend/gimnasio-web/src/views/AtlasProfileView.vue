<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { getAtlasOverview, updateAtlasProfile } from '../services/atlasService'
import type { AtlasOverview, ProfileFact, UpdateAtlasProfileRequest } from '../types/atlas'

const overview = ref<AtlasOverview | null>(null)
const loading = ref(true)
const saving = ref(false)
const editing = ref(false)
const message = ref('')
const error = ref('')
const form = reactive<UpdateAtlasProfileRequest>({ displayName: '', heightCm: null, referenceWeightKg: null, primaryGoal: '', targetDate: null, dominantHand: null, dominantLeg: null, affectedKnee: null })

const groupedFacts = computed(() => {
  const groups = new Map<string, ProfileFact[]>()
  for (const fact of overview.value?.facts ?? []) groups.set(fact.category, [...(groups.get(fact.category) ?? []), fact])
  return [...groups.entries()]
})

function fillForm() {
  if (!overview.value) return
  const { id: _, ...profile } = overview.value.profile
  Object.assign(form, profile)
}

async function load() {
  loading.value = true
  try { overview.value = await getAtlasOverview(); fillForm() }
  catch (cause) { error.value = cause instanceof Error ? cause.message : 'No se pudo cargar el perfil.' }
  finally { loading.value = false }
}

async function save() {
  saving.value = true
  error.value = ''
  try {
    const profile = await updateAtlasProfile(form)
    if (overview.value) overview.value.profile = profile
    editing.value = false
    message.value = 'Perfil actualizado. Los antecedentes conservan por separado su fuente y grado de confirmación.'
  } catch (cause) { error.value = cause instanceof Error ? cause.message : 'No se pudo actualizar el perfil.' }
  finally { saving.value = false }
}

onMounted(load)
</script>

<template>
  <section class="page atlas-profile-page" :class="{ muted: loading }">
    <div class="page-heading">
      <div><span class="eyebrow">Proyecto Atlas · Identidad funcional</span><h1>Mi perfil y línea de base</h1><p>Cada afirmación conserva su procedencia y su grado real de confirmación.</p></div>
      <button v-if="!editing" class="primary-button" @click="editing = true">Editar datos base</button>
    </div>
    <div v-if="error" class="notice error">{{ error }}</div>
    <div v-if="message" class="notice atlas-success">{{ message }}</div>

    <form v-if="editing" class="panel atlas-profile-form" @submit.prevent="save">
      <div class="panel-heading"><div><span class="eyebrow">Datos base</span><h2>Información personal</h2></div><span class="evidence-chip">Editable</span></div>
      <div class="atlas-form-grid profile-grid">
        <label>Nombre visible<input v-model="form.displayName" required></label>
        <label>Altura (cm)<input v-model.number="form.heightCm" type="number" min="80" max="260" step="0.1"></label>
        <label>Peso de referencia (kg)<input v-model.number="form.referenceWeightKg" type="number" min="20" max="400" step="0.1"></label>
        <label>Fecha objetivo<input v-model="form.targetDate" type="date"></label>
        <label>Mano dominante<select v-model="form.dominantHand"><option :value="null">Pendiente</option><option>derecha</option><option>izquierda</option><option>ambidiestra</option></select></label>
        <label>Pierna dominante<select v-model="form.dominantLeg"><option :value="null">Pendiente</option><option>derecha</option><option>izquierda</option><option>sin predominio</option></select></label>
        <label>Rodilla afectada<select v-model="form.affectedKnee"><option :value="null">Pendiente de confirmar</option><option>derecha</option><option>izquierda</option><option>bilateral</option></select></label>
        <label class="wide">Objetivo principal<textarea v-model="form.primaryGoal" rows="3"></textarea></label>
      </div>
      <div class="form-actions"><button type="button" class="secondary-button" @click="editing = false; fillForm()">Cancelar</button><button class="primary-button" :disabled="saving">{{ saving ? 'Guardando…' : 'Guardar perfil' }}</button></div>
    </form>

    <div v-else class="atlas-profile-summary">
      <article class="panel atlas-identity-card"><span class="eyebrow">Atleta</span><h2>{{ overview?.profile.displayName }}</h2><p>{{ overview?.profile.primaryGoal }}</p><div class="atlas-vitals"><div><strong>{{ overview?.profile.heightCm ?? '—' }}</strong><span>cm</span></div><div><strong>{{ overview?.profile.referenceWeightKg ?? '—' }}</strong><span>kg de referencia</span></div><div><strong>{{ overview?.profile.affectedKnee ?? 'Pendiente' }}</strong><span>rodilla afectada</span></div></div></article>
      <article class="panel atlas-quality-card"><span class="eyebrow">Calidad de la información</span><h2>{{ overview?.facts.length ?? 0 }} hechos registrados</h2><p>“Pendiente” no significa “no”. Atlas nunca completa silenciosamente un dato ausente.</p><div class="atlas-legend"><span class="fact-status confirmed">Confirmado</span><span class="fact-status selfreported">Autorreportado</span><span class="fact-status pending">Pendiente</span></div></article>
    </div>

    <div class="atlas-facts">
      <article v-for="[category, facts] in groupedFacts" :key="category" class="panel atlas-fact-group">
        <div class="panel-heading"><div><span class="eyebrow">Expediente</span><h2>{{ category }}</h2></div><span class="badge">{{ facts.length }}</span></div>
        <div class="atlas-fact-list">
          <div v-for="fact in facts" :key="fact.id" class="atlas-fact">
            <div><strong>{{ fact.label }}</strong><p>{{ fact.value ?? 'Información pendiente de completar' }}</p><small v-if="fact.notes">{{ fact.notes }}</small></div>
            <div class="atlas-fact-meta"><span class="fact-status" :class="fact.status.toLowerCase()">{{ fact.statusLabel }}</span><small>{{ fact.sourceTitle }}</small></div>
          </div>
        </div>
      </article>
    </div>
  </section>
</template>
