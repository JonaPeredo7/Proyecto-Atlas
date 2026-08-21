<script setup lang="ts">
import { computed, onMounted, onUnmounted, reactive, ref } from 'vue'
import { deleteDailyActivity, getAtlasOverview, saveDailyActivity, saveDailyCheckInResilient, saveDailyPlanDecision } from '../services/atlasService'
import { useAuthStore } from '../stores/auth'
import type { AtlasOverview, DailyActivity, SaveDailyActivityRequest, SaveDailyCheckInRequest, SaveDailyPlanDecisionRequest, TodaySchedule } from '../types/atlas'

const overview = ref<AtlasOverview | null>(null)
const loading = ref(true)
const saving = ref(false)
const error = ref('')
const success = ref('')
const hasPain = ref(false)
const savingActivity = ref(false)
const editingActivityId = ref<string | null>(null)
const savingDecision=ref(false)
const today = new Date().toLocaleDateString('en-CA')
const auth = useAuthStore()

const form = reactive<SaveDailyCheckInRequest>({
  date: today,
  sleepMinutes: 420,
  sleepQuality: 3,
  energy: 3,
  fatigue: 4,
  stress: 3,
  painLocation: null,
  painSide: null,
  painIntensity: null,
  stiffness: 'ninguna',
  swelling: 'ninguna',
  instability: false,
  locking: false,
  expectedWorkLoad: 5,
  plannedCyclingKm: 5,
  plannedActivity: '',
  notes: '',
})
const activityForm = reactive<SaveDailyActivityRequest>({ date: today, activityType: 'Trabajo físico', durationMinutes: 60, rpe: 4, distanceKm: null, notes: '',plannedDurationMinutes:null,plannedSource:null,workDemands:null,breakMinutes:null,unusualConditions:null })
const decisionForm=reactive<SaveDailyPlanDecisionRequest>({decision:'as-planned',reason:''})

const sleepHours = computed({
  get: () => form.sleepMinutes === null ? null : Number((form.sleepMinutes / 60).toFixed(1)),
  set: (value: number | null) => { form.sleepMinutes = value === null ? null : Math.round(value * 60) },
})
const isWorkActivity=computed(()=>activityForm.activityType.toLowerCase().includes('trabajo'))

const statusMessage = computed(() => {
  const checkIn = overview.value?.today
  if (!checkIn) return 'Todavía no registraste cómo estás hoy.'
  if (checkIn.needsAttention) return 'Hay señales que merecen atención y seguimiento, sin asumir un diagnóstico.'
  return 'Tu registro de hoy está completo. Atlas lo comparará con tu propia línea de base.'
})
const completedActions = computed(() => overview.value?.hub.actions.filter(item => item.state === 'done').length ?? 0)
const actionableItems = computed(() => overview.value?.hub.actions.filter(item => item.state !== 'optional').length ?? 0)
const urgentAction = computed(() => overview.value?.hub.actions.find(item => item.state === 'attention') ?? null)
function clock(value:string|null){return value?.slice(0,5)??''}
function scheduleWindow(item:{timeWindow:string;startTime:string|null;endTime:string|null}){if(item.startTime&&item.endTime)return`${clock(item.startTime)}–${clock(item.endTime)}`;return item.timeWindow==='morning'?'Por la mañana':item.timeWindow==='afternoon'?'Por la tarde':item.timeWindow==='evening'?'Por la noche':'Horario flexible'}
function scheduleMinutes(item:TodaySchedule){if(!item.startTime||!item.endTime)return 60;const [sh,sm]=item.startTime.split(':').map(Number),[eh,em]=item.endTime.split(':').map(Number);return Math.max(1,(eh*60+em)-(sh*60+sm))}
function prepareScheduledActivity(item:TodaySchedule){const planned=scheduleMinutes(item);Object.assign(activityForm,{date:today,activityType:'Trabajo físico',durationMinutes:planned,rpe:4,distanceKm:null,notes:'',plannedDurationMinutes:planned,plannedSource:`${item.name} · ${scheduleWindow(item)}`,workDemands:null,breakMinutes:null,unusualConditions:null});document.querySelector('#carga-diaria')?.scrollIntoView({behavior:'smooth'})}

function hydrate(data: AtlasOverview) {
  overview.value = data
  if (!data.today) return
  Object.assign(form, data.today)
  hasPain.value = data.today.painIntensity !== null || Boolean(data.today.painLocation)
  if(data.hub.decision)Object.assign(decisionForm,{decision:data.hub.decision.decision,reason:data.hub.decision.reason})
}

async function load() {
  loading.value = true
  error.value = ''
  try { hydrate(await getAtlasOverview()) }
  catch (cause) { error.value = cause instanceof Error ? cause.message : 'No se pudo abrir Atlas.' }
  finally { loading.value = false }
}

async function submit() {
  saving.value = true
  error.value = ''
  success.value = ''
  if (!hasPain.value) {
    form.painLocation = null
    form.painSide = null
    form.painIntensity = null
    form.stiffness = 'ninguna'
    form.swelling = 'ninguna'
    form.instability = false
    form.locking = false
  }
  try {
    if (!auth.user) throw new Error('La sesión debe estar activa para guardar un pendiente.')
    const result = await saveDailyCheckInResilient(form, auth.user.id)
    if (result.queued) { success.value = 'Check-in guardado en este dispositivo. Atlas lo sincronizará al recuperar conexión.'; return }
    if (overview.value) overview.value.today = result.data
    await load()
    success.value = 'Check-in guardado. El dato quedó registrado como autorreporte de hoy.'
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : 'No se pudo guardar el check-in.'
  } finally { saving.value = false }
}

function resetActivity() {
  editingActivityId.value = null
  Object.assign(activityForm, { date: today, activityType: 'Trabajo físico', durationMinutes: 60, rpe: 4, distanceKm: null, notes: '',plannedDurationMinutes:null,plannedSource:null,workDemands:null,breakMinutes:null,unusualConditions:null })
}

function editActivity(item: DailyActivity) {
  editingActivityId.value = item.id
  Object.assign(activityForm, { date: item.date, activityType: item.activityType, durationMinutes: item.durationMinutes, rpe: item.rpe, distanceKm: item.distanceKm, notes: item.notes ?? '',plannedDurationMinutes:item.plannedDurationMinutes,plannedSource:item.plannedSource,workDemands:item.workDemands,breakMinutes:item.breakMinutes,unusualConditions:item.unusualConditions })
}

async function submitActivity() {
  savingActivity.value = true
  error.value = ''
  try {
    await saveDailyActivity(activityForm, editingActivityId.value ?? undefined)
    resetActivity()
    await load()
    success.value = 'Actividad cotidiana guardada. Atlas actualizó la carga total del día.'
  } catch (cause) { error.value = cause instanceof Error ? cause.message : 'No se pudo guardar la actividad.' }
  finally { savingActivity.value = false }
}

async function removeActivity(id: string) {
  error.value = ''
  try { await deleteDailyActivity(id); if (editingActivityId.value === id) resetActivity(); await load() }
  catch (cause) { error.value = cause instanceof Error ? cause.message : 'No se pudo eliminar la actividad.' }
}

async function submitDecision(){savingDecision.value=true;error.value='';success.value='';try{await saveDailyPlanDecision(decisionForm);await load();success.value='Decisión del día registrada junto con el contexto disponible.'}catch(cause){error.value=cause instanceof Error?cause.message:'No se pudo registrar la decisión.'}finally{savingDecision.value=false}}

function reloadAfterSync() { void load() }
onMounted(() => { void load(); window.addEventListener('atlas-sync-complete', reloadAfterSync) })
onUnmounted(() => window.removeEventListener('atlas-sync-complete', reloadAfterSync))
</script>

<template>
  <section class="page atlas-page">
    <div class="atlas-hero">
      <div>
        <span class="eyebrow light">Proyecto Atlas · Hoy</span>
        <h1>Buen día, {{ overview?.profile.displayName ?? 'Jonathan' }}.</h1>
        <p>{{ statusMessage }}</p>
      </div>
      <div class="atlas-date"><span>{{ new Intl.DateTimeFormat('es-AR', { weekday: 'long' }).format(new Date()) }}</span><strong>{{ new Intl.DateTimeFormat('es-AR', { day: '2-digit', month: 'short' }).format(new Date()) }}</strong></div>
    </div>

    <div v-if="error" class="notice error">{{ error }}</div>
    <div v-if="success" class="notice atlas-success">{{ success }}</div>

    <section v-if="overview" class="today-command" :class="{ muted: loading }">
      <article class="panel today-priority">
        <div class="panel-heading">
          <div><span class="eyebrow">Centro operativo</span><h2>Tu día en Atlas</h2></div>
          <span class="today-progress">{{ completedActions }}/{{ actionableItems }} esenciales</span>
        </div>
        <div v-if="urgentAction" class="today-alert"><span>!</span><div><strong>{{ urgentAction.title }}</strong><p>{{ urgentAction.detail }}</p></div><RouterLink :to="urgentAction.route">Atender</RouterLink></div>
        <div class="today-actions">
          <RouterLink v-for="item in overview.hub.actions" :key="`${item.kind}-${item.title}`" :to="item.route" class="today-action" :class="item.state">
            <span class="today-action-icon">{{ item.state === 'done' ? '✓' : item.kind === 'training' ? 'T' : item.kind === 'follow-up' ? '24' : item.kind === 'measurement' ? 'M' : item.kind === 'knee' ? 'R' : item.kind === 'weekly' ? '7' : item.kind === 'plan' ? 'P' : item.kind.startsWith('learning') ? 'B' : item.kind === 'planning' ? '+' : '•' }}</span>
            <span><strong>{{ item.title }}</strong><small>{{ item.detail }}</small></span>
            <em>{{ item.state === 'done' ? 'Listo' : item.state === 'active' ? 'En curso' : item.state === 'attention' ? 'Revisar' : item.state === 'optional' ? 'Sugerido' : 'Pendiente' }}</em>
          </RouterLink>
        </div>
      </article>
      <aside class="today-snapshot">
        <article><span>Sesiones hoy</span><strong>{{ overview.hub.todaySessions.length }}</strong><small>{{ overview.hub.todaySessions[0]?.name ?? 'Sin sesión programada' }}</small></article>
        <article :class="{ attention: overview.hub.pendingFollowUps > 0 }"><span>Respuesta 24 h</span><strong>{{ overview.hub.pendingFollowUps }}</strong><small>{{ overview.hub.pendingFollowUps ? 'Controles pendientes' : 'Sin pendientes' }}</small></article>
        <article :class="{ attention: overview.hub.dueLearningActions > 0 }"><span>Acciones de bitácora</span><strong>{{ overview.hub.openLearningActions }}</strong><small>{{ overview.hub.dueLearningActions ? `${overview.hub.dueLearningActions} para revisar` : overview.hub.openLearningActions ? 'Abiertas, sin vencimiento hoy' : 'Sin acciones abiertas' }}</small></article>
        <article><span>Carga total hoy</span><strong>{{ overview.hub.totalLoadToday }}</strong><small>{{ overview.hub.trainingLoadToday }} entrenamiento + {{ overview.hub.externalLoadToday }} externa</small></article>
        <article><span>Objetivo principal</span><strong>{{ overview.hub.daysToPrimaryTarget ?? '—' }}</strong><small>{{ overview.hub.daysToPrimaryTarget !== null ? 'días restantes' : 'Sin fecha definida' }}</small></article>
      </aside>
    </section>

    <section v-if="overview" class="panel today-schedule" :class="{attention:overview.hub.hasScheduleConflict}"><header><div><span class="eyebrow">Estructura de la jornada</span><h2>Compromisos de hoy</h2><p v-if="overview.hub.todaySchedule.length">{{Math.round(overview.hub.scheduledMinutes/60*10)/10}} horas previstas en bloques recurrentes.</p><p v-else>No hay compromisos recurrentes cargados para hoy.</p></div><RouterLink to="/agenda" class="secondary-button">Abrir agenda</RouterLink></header><div v-if="overview.hub.todaySchedule.length" class="today-schedule-list"><article v-for="item in overview.hub.todaySchedule" :key="item.id" :class="item.category"><span>{{scheduleWindow(item)}}</span><strong>{{item.name}}</strong><small>{{item.notes}}</small><button v-if="item.category==='work'" class="small-button" @click="prepareScheduledActivity(item)">Registrar jornada</button><RouterLink v-else-if="item.category==='training'" class="small-button" :to="{path:'/agenda',query:{date:today,plan:item.id}}">Preparar sesión</RouterLink></article></div><p v-if="overview.hub.hasScheduleConflict" class="today-schedule-warning">Hay horarios superpuestos. Revisá la agenda antes de confirmar el plan del día.</p></section>

    <section v-if="overview" class="panel personal-state" :class="overview.hub.state.status">
      <header><div><span class="eyebrow">Contexto personal</span><h2>Cómo se presenta el día</h2><p>{{ overview.hub.state.summary }}</p></div><span>{{ overview.hub.state.label }}</span></header>
      <div v-if="overview.hub.state.factors.length" class="personal-state-factors">
        <article v-for="factor in overview.hub.state.factors" :key="factor.key" :class="factor.trend"><div><span>{{ factor.label }}</span><strong>{{ factor.current }}{{ factor.unit }}</strong></div><p>Referencia {{ factor.baseline }}{{ factor.unit }} · diferencia {{ factor.delta>0?'+':'' }}{{ factor.delta }}{{ factor.unit }}</p><em>{{ factor.trend==='better'?'Más favorable':factor.trend==='worse'?'Menos favorable':'Similar' }}</em><small>Cambia desde ±{{ factor.visualThreshold }}{{ factor.unit }}. {{ factor.basis }}</small></article>
      </div>
      <footer><span>{{ overview.hub.state.baselineDays }} días previos comparables</span><p>{{ overview.hub.state.disclaimer }}</p></footer>
    </section>

    <section v-if="overview" class="panel daily-plan-context" :class="overview.hub.planContext.status">
      <div><span class="eyebrow">Plan previsto</span><h2>{{ overview.hub.planContext.label }}</h2><p>{{ overview.hub.planContext.summary }}</p><small>{{ overview.hub.planContext.disclaimer }}</small></div>
      <div class="daily-plan-numbers"><article><span>Sesiones</span><strong>{{ overview.hub.planContext.sessionCount }}</strong></article><article><span>Minutos previstos</span><strong>{{ overview.hub.planContext.plannedMinutes || '—' }}</strong></article><article><span>Carga prevista</span><strong>{{ overview.hub.planContext.plannedLoad || '—' }}</strong><small>duración × RPE</small></article><article :class="{attention:overview.hub.planContext.incompleteSessions}"><span>Datos incompletos</span><strong>{{ overview.hub.planContext.incompleteSessions }}</strong></article></div>
      <RouterLink to="/entrenamiento" class="secondary-button">{{ overview.hub.planContext.hasInProgress?'Continuar sesión':'Revisar entrenamiento' }}</RouterLink>
    </section>

    <form v-if="overview" id="decision-dia" class="panel daily-decision" @submit.prevent="submitDecision">
      <header><div><span class="eyebrow">Decisión personal</span><h2>¿Qué elegís hacer hoy?</h2><p>Registrá una decisión consciente después de revisar tu contexto y el plan. Atlas no elige por vos.</p></div><span v-if="overview.hub.decision">Versión {{ overview.hub.decision.version }}</span></header>
      <div class="decision-options">
        <label :class="{selected:decisionForm.decision==='as-planned'}"><input v-model="decisionForm.decision" type="radio" value="as-planned"><span><strong>Mantener el plan</strong><small>Realizar la sesión tal como está prevista.</small></span></label>
        <label :class="{selected:decisionForm.decision==='adjusted'}"><input v-model="decisionForm.decision" type="radio" value="adjusted"><span><strong>Ajustar el plan</strong><small>Corregir volumen, intensidad o ejercicios explícitamente.</small></span></label>
        <label :class="{selected:decisionForm.decision==='recovery'}"><input v-model="decisionForm.decision" type="radio" value="recovery"><span><strong>Priorizar recuperación</strong><small>Elegir descanso o actividad de recuperación.</small></span></label>
        <label :class="{selected:decisionForm.decision==='professional-review'}"><input v-model="decisionForm.decision" type="radio" value="professional-review"><span><strong>Consultar antes</strong><small>Revisar la situación con un profesional.</small></span></label>
      </div>
      <div class="decision-reason"><label>Motivo de la decisión<textarea v-model="decisionForm.reason" rows="3" maxlength="1000" required placeholder="Ej.: dormí peor que mi referencia y voy a revisar el volumen antes de empezar"></textarea></label><button class="primary-button" :disabled="savingDecision||!decisionForm.reason.trim()">{{ savingDecision?'Guardando…':overview.hub.decision?'Actualizar decisión':'Registrar decisión' }}</button></div>
      <footer>Se guardarán el estado de contexto y la carga prevista actuales para interpretar esta decisión más adelante.</footer>
    </form>

    <section v-if="overview" id="carga-diaria" class="panel daily-load-panel">
      <div class="panel-heading"><div><span class="eyebrow">Carga externa realizada</span><h2>Trabajo, bicicleta y actividad cotidiana</h2></div><span class="daily-load-total">{{ overview.hub.externalLoadToday }} UA</span></div>
      <p class="atlas-form-intro">Registrá sólo actividad física relevante ya realizada. Atlas calcula duración × RPE y conserva su origen separado del entrenamiento.</p>
      <div class="daily-load-layout">
        <form class="daily-activity-form" @submit.prevent="submitActivity">
          <div class="atlas-form-grid">
            <label>Tipo<select v-model="activityForm.activityType"><option>Trabajo físico</option><option>Bicicleta</option><option>Caminata</option><option>Taekwondo informal</option><option>Movilidad</option><option>Otra actividad</option></select></label>
            <label>Fecha<input v-model="activityForm.date" type="date" required></label>
            <label>Duración (min)<input v-model.number="activityForm.durationMinutes" type="number" min="1" max="960" required></label>
            <label>RPE realizado<select v-model.number="activityForm.rpe"><option v-for="n in 10" :key="n" :value="n">{{ n }} / 10</option></select></label>
            <label>Distancia (km, opcional)<input v-model.number="activityForm.distanceKm" type="number" min="0" max="500" step="0.1"></label>
            <label>Notas<input v-model="activityForm.notes" placeholder="Ej.: escaleras y traslado de cargas"></label>
            <template v-if="isWorkActivity"><label class="wide">Demandas principales<input v-model="activityForm.workDemands" maxlength="300" placeholder="Ej.: caminata, escaleras, tiempo de pie y traslado de cargas"></label><label>Pausas totales (min)<input v-model.number="activityForm.breakMinutes" type="number" min="0" :max="activityForm.durationMinutes" placeholder="Opcional"></label><label class="wide">Condiciones inusuales<input v-model="activityForm.unusualConditions" maxlength="400" placeholder="Ej.: se agregó otro lugar, más escaleras o cargas mayores"></label></template>
          </div>
          <div class="daily-load-actions"><button v-if="editingActivityId" type="button" class="secondary-button" @click="resetActivity">Cancelar edición</button><button class="primary-button" :disabled="savingActivity">{{ savingActivity ? 'Guardando…' : editingActivityId ? 'Actualizar actividad' : 'Agregar actividad' }}</button></div>
        </form>
        <div class="daily-activity-list">
          <article v-for="item in overview.hub.todayActivities" :key="item.id">
            <div><strong>{{ item.activityType }}</strong><small>{{ item.durationMinutes }} min · RPE {{ item.rpe }}<template v-if="item.distanceKm !== null"> · {{ item.distanceKm }} km</template></small><p v-if="item.plannedDurationMinutes!==null" class="activity-plan-comparison">Previsto {{item.plannedDurationMinutes}} min · <b>{{item.durationVarianceMinutes===0?'sin diferencia':(item.durationVarianceMinutes??0)>0?`+${item.durationVarianceMinutes} min extra`:`${item.durationVarianceMinutes} min`}}</b></p><div v-if="item.workDemands||item.breakMinutes!==null||item.unusualConditions" class="activity-work-context"><span v-if="item.workDemands">{{item.workDemands}}</span><small v-if="item.breakMinutes!==null">Pausas: {{item.breakMinutes}} min</small><em v-if="item.unusualConditions">Inusual: {{item.unusualConditions}}</em></div><p v-if="item.notes">{{ item.notes }}</p></div>
            <span>{{ item.internalLoad }} UA</span>
            <div class="row-actions"><button class="small-button" @click="editActivity(item)">Editar</button><button class="small-button danger" @click="removeActivity(item.id)">Eliminar</button></div>
          </article>
          <p v-if="!overview.hub.todayActivities.length" class="list-empty">Todavía no registraste carga externa realizada hoy.</p>
        </div>
      </div>
    </section>

    <div class="atlas-layout" :class="{ muted: loading }">
      <form class="panel atlas-checkin" @submit.prevent="submit">
        <div class="panel-heading">
          <div><span class="eyebrow">Check-in diario</span><h2>¿Cómo está tu cuerpo hoy?</h2></div>
          <span class="evidence-chip">Autorreporte</span>
        </div>
        <p class="atlas-form-intro">Este registro describe tu estado. No es una evaluación médica ni decide por sí solo si debés entrenar.</p>

        <div class="atlas-form-grid">
          <label>Horas de sueño<input v-model.number="sleepHours" type="number" min="0" max="24" step="0.5"></label>
          <label>Calidad del sueño<select v-model.number="form.sleepQuality"><option v-for="n in 5" :key="n" :value="n">{{ n }} / 5</option></select></label>
          <label>Energía<select v-model.number="form.energy"><option v-for="n in 5" :key="n" :value="n">{{ n }} / 5</option></select></label>
          <label>Fatiga física<select v-model.number="form.fatigue"><option v-for="n in 11" :key="n - 1" :value="n - 1">{{ n - 1 }} / 10</option></select></label>
          <label>Estrés<select v-model.number="form.stress"><option v-for="n in 5" :key="n" :value="n">{{ n }} / 5</option></select></label>
          <label>Carga laboral esperada<select v-model.number="form.expectedWorkLoad"><option v-for="n in 11" :key="n - 1" :value="n - 1">{{ n - 1 }} / 10</option></select></label>
          <label>Bicicleta prevista (km)<input v-model.number="form.plannedCyclingKm" type="number" min="0" max="500" step="0.5"></label>
          <label>Actividad prevista<input v-model="form.plannedActivity" placeholder="Ej.: taekwondo, gimnasio o descanso"></label>
        </div>

        <label class="atlas-pain-toggle"><input v-model="hasPain" type="checkbox"><span><strong>Tengo dolor o una molestia</strong><small>Al marcarlo aparecen preguntas específicas.</small></span></label>

        <div v-if="hasPain" class="atlas-symptom-box">
          <div class="atlas-form-grid">
            <label>Zona<input v-model="form.painLocation" placeholder="Ej.: rodilla"></label>
            <label>Lado<select v-model="form.painSide"><option :value="null">Sin definir</option><option>derecho</option><option>izquierdo</option><option>bilateral</option></select></label>
            <label>Intensidad<select v-model.number="form.painIntensity"><option :value="null">Sin registrar</option><option v-for="n in 11" :key="n - 1" :value="n - 1">{{ n - 1 }} / 10</option></select></label>
            <label>Rigidez<select v-model="form.stiffness"><option>ninguna</option><option>leve</option><option>moderada</option><option>alta</option></select></label>
            <label>Inflamación<select v-model="form.swelling"><option>ninguna</option><option>leve</option><option>moderada</option><option>alta</option></select></label>
          </div>
          <div class="atlas-flag-row">
            <label><input v-model="form.instability" type="checkbox"> Sensación de falseo</label>
            <label><input v-model="form.locking" type="checkbox"> Bloqueo</label>
          </div>
        </div>

        <label>Nota breve<textarea v-model="form.notes" rows="3" placeholder="Algo que las escalas no expliquen"></textarea></label>
        <div class="atlas-submit-row"><span>Tiempo estimado: menos de 90 segundos.</span><button class="primary-button" :disabled="saving">{{ saving ? 'Guardando…' : 'Guardar check-in' }}</button></div>
      </form>

      <aside class="atlas-side">
        <article class="panel atlas-context-card">
          <span class="eyebrow">Objetivo principal</span>
          <h2>{{ overview?.profile.primaryGoal }}</h2>
          <p v-if="overview?.profile.targetDate">Fecha de referencia: {{ new Intl.DateTimeFormat('es-AR').format(new Date(`${overview.profile.targetDate}T00:00:00`)) }}</p>
          <RouterLink to="/mi-perfil" class="text-link dark-link">Revisar mi perfil →</RouterLink>
        </article>
        <article class="panel atlas-evidence-card">
          <span class="eyebrow">Gobernanza científica</span>
          <h2>Evidencia bajo control</h2>
          <div class="atlas-evidence-row"><span>Reglas operativas</span><strong>{{ overview?.evidence.operational ?? 0 }}</strong></div>
          <div class="atlas-evidence-row"><span>En revisión</span><strong>{{ overview?.evidence.inReview ?? 0 }}</strong></div>
          <p>Atlas no activará recomendaciones médicas sin fuente, límites, versión y revisión documentada.</p>
        </article>
      </aside>
    </div>
  </section>
</template>

<style scoped>
.personal-state-factors small{display:block;margin-top:7px;color:#7d8998;font-size:6px;line-height:1.45}
</style>
