<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { createSession, getSchedule, getTrainingCalendar, removeScheduleBlock, saveScheduleBlock } from '../services/trainingService'
import type { ScheduleBlock, TrainingCalendar, TrainingCalendarDay } from '../types/training'

const data = ref<TrainingCalendar | null>(null)
const anchor = ref(new Date())
const mode = ref<'week' | 'month'>('week')
const selectedDate = ref('')
const showPlan = ref(false)
const loading = ref(true)
const error = ref('')
const success = ref('')
const route=useRoute()
const schedule = ref<ScheduleBlock[]>([])
const showSchedule = ref(false)
const savingSchedule = ref(false)
const form = reactive({ date: '', name: '', activityType: 'Fuerza', plannedStartTime:null as string|null, plannedDurationMinutes: 60, targetRpe: 6, goal: '', notes: '',personalGoalId:null,trainingCycleId:null,changeReason:'Planificación rápida desde la agenda' })
const scheduleForm = reactive({name:'',category:'training',daysOfWeek:[] as number[],timeWindow:'exact',startTime:'09:00',endTime:'10:00',effectiveFrom:new Date().toLocaleDateString('en-CA'),effectiveTo:null as string|null,notes:''})

function iso(d: Date) { const y = d.getFullYear(), m = String(d.getMonth() + 1).padStart(2, '0'), day = String(d.getDate()).padStart(2, '0'); return `${y}-${m}-${day}` }
function fromIso(value: string) { return new Date(`${value}T12:00:00`) }
function add(d: Date, days: number) { const result = new Date(d); result.setDate(result.getDate() + days); return result }
function weekStart(d: Date) { const result = new Date(d), day = result.getDay() || 7; result.setDate(result.getDate() - day + 1); return result }

const range = computed(() => {
  if (mode.value === 'week') { const from = weekStart(anchor.value); return { from, to: add(from, 6) } }
  const first = new Date(anchor.value.getFullYear(), anchor.value.getMonth(), 1), from = weekStart(first)
  return { from, to: add(from, 41) }
})
const title = computed(() => mode.value === 'week'
  ? `${range.value.from.toLocaleDateString('es-AR', { day: '2-digit', month: 'short' })} — ${range.value.to.toLocaleDateString('es-AR', { day: '2-digit', month: 'short', year: 'numeric' })}`
  : anchor.value.toLocaleDateString('es-AR', { month: 'long', year: 'numeric' }))
const selected = computed(() => data.value?.days.find(item => item.date === selectedDate.value) ?? null)
const weekdays = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom']
const today = iso(new Date())
function outside(value: string) { return mode.value === 'month' && fromIso(value).getMonth() !== anchor.value.getMonth() }

async function load() {
  loading.value = true; error.value = ''
  try {
    ;[data.value,schedule.value] = await Promise.all([getTrainingCalendar(iso(range.value.from), iso(range.value.to)),getSchedule()])
    if (!selectedDate.value || !data.value.days.some(item => item.date === selectedDate.value)) selectedDate.value = data.value.days.find(item => item.date === today)?.date ?? data.value.days[0]?.date ?? ''
  } catch (cause) { error.value = cause instanceof Error ? cause.message : 'No se pudo cargar la agenda.' }
  finally { loading.value = false }
}
async function move(value: number) { anchor.value = mode.value === 'week' ? add(anchor.value, value * 7) : new Date(anchor.value.getFullYear(), anchor.value.getMonth() + value, 1); selectedDate.value = ''; await load() }
async function changeMode(next: 'week' | 'month') { mode.value = next; selectedDate.value = ''; await load() }
function openPlan(day: TrainingCalendarDay) { selectedDate.value = day.date; form.date = day.date; showPlan.value = true }
function minutesBetween(start:string|null,end:string|null){if(!start||!end)return 60;const [sh,sm]=start.split(':').map(Number),[eh,em]=end.split(':').map(Number);return Math.max(1,(eh*60+em)-(sh*60+sm))}
function openPlanFromBlock(block:ScheduleBlock){form.date=selectedDate.value;form.name=block.name;form.activityType=block.name.toLowerCase().includes('taekwondo')?'Taekwondo':block.name.toLowerCase().includes('gimnas')?'Fuerza':'Otra';form.plannedStartTime=clock(block.startTime)||null;form.plannedDurationMinutes=minutesBetween(block.startTime,block.endTime);form.notes=`Sesión preparada desde el horario recurrente: ${block.name}.`;showPlan.value=true}
async function plan() {
  try {
    await createSession(form); success.value = 'Sesión agregada a la agenda.'; showPlan.value = false
    Object.assign(form, { date: selectedDate.value, name: '', activityType: 'Fuerza', plannedStartTime:null, plannedDurationMinutes: 60, targetRpe: 6, goal: '', notes: '',personalGoalId:null,trainingCycleId:null,changeReason:'Planificación rápida desde la agenda' }); await load()
  } catch (cause) { error.value = cause instanceof Error ? cause.message : 'No se pudo planificar.' }
}
const dayNames=['','Lunes','Martes','Miércoles','Jueves','Viernes','Sábado','Domingo']
function clock(value:string|null){return value?.slice(0,5)??''}
function windowLabel(block:ScheduleBlock){return block.timeWindow==='exact'?`${clock(block.startTime)}–${clock(block.endTime)}`:block.timeWindow==='morning'?'Mañana':block.timeWindow==='afternoon'?'Tarde':block.timeWindow==='evening'?'Noche':'Flexible'}
async function saveBlock(){savingSchedule.value=true;error.value='';try{await saveScheduleBlock({...scheduleForm,startTime:scheduleForm.timeWindow==='exact'?scheduleForm.startTime:null,endTime:scheduleForm.timeWindow==='exact'?scheduleForm.endTime:null,effectiveTo:scheduleForm.effectiveTo||null});success.value='Horario recurrente guardado.';showSchedule.value=false;await load()}catch(cause){error.value=cause instanceof Error?cause.message:'No se pudo guardar el horario.'}finally{savingSchedule.value=false}}
async function removeBlock(id:string){try{await removeScheduleBlock(id);success.value='Bloque horario quitado.';await load()}catch(cause){error.value=cause instanceof Error?cause.message:'No se pudo quitar el horario.'}}
function prepareOneOff(){if(!selectedDate.value)return;const day=fromIso(selectedDate.value).getDay()||7;Object.assign(scheduleForm,{name:'Trabajo extra',category:'work',daysOfWeek:[day],timeWindow:'exact',startTime:'19:00',endTime:'20:00',effectiveFrom:selectedDate.value,effectiveTo:selectedDate.value,notes:'Extensión o compromiso puntual'});showSchedule.value=true;scrollTo({top:0,behavior:'smooth'})}
async function loadPersonalSchedule(){savingSchedule.value=true;error.value='';try{await Promise.all([
  saveScheduleBlock({name:'Trabajo',category:'work',daysOfWeek:[1],timeWindow:'exact',startTime:'13:00',endTime:'20:00',effectiveFrom:'2026-08-15',effectiveTo:null,notes:'Horario laboral habitual de los lunes'}),
  saveScheduleBlock({name:'Trabajo',category:'work',daysOfWeek:[2,3,4],timeWindow:'exact',startTime:'13:00',endTime:'19:00',effectiveFrom:'2026-08-15',effectiveTo:null,notes:'Horario laboral habitual de martes a jueves'}),
  saveScheduleBlock({name:'Trabajo',category:'work',daysOfWeek:[5],timeWindow:'exact',startTime:'13:00',endTime:'19:30',effectiveFrom:'2026-08-15',effectiveTo:null,notes:'Horario laboral habitual de los viernes'}),
  saveScheduleBlock({name:'Taekwondo',category:'training',daysOfWeek:[2,4],timeWindow:'exact',startTime:'19:00',endTime:'20:30',effectiveFrom:'2026-08-15',effectiveTo:null,notes:'Clase habitual'}),
  saveScheduleBlock({name:'Gimnasio',category:'training',daysOfWeek:[1,3,5],timeWindow:'exact',startTime:'09:00',endTime:'10:00',effectiveFrom:'2026-09-01',effectiveTo:null,notes:'Inicio previsto para septiembre'})
]);success.value='Tu horario personal quedó cargado en la agenda.';await load()}catch(cause){error.value=cause instanceof Error?cause.message:'No se pudo cargar el horario personal.'}finally{savingSchedule.value=false}}
onMounted(async()=>{await load();const requestedDate=String(route.query.date??''),blockId=String(route.query.plan??'');if(requestedDate&&data.value?.days.some(x=>x.date===requestedDate))selectedDate.value=requestedDate;if(blockId){const block=data.value?.days.find(x=>x.date===selectedDate.value)?.scheduleBlocks.find(x=>x.id===blockId);if(block)openPlanFromBlock(block)}})
</script>

<template>
  <section class="page calendar-page" :class="{ muted: loading }">
    <div class="page-heading"><div><span class="eyebrow">Planificación temporal</span><h1>Agenda Atlas</h1><p>Entrenamiento, actividad cotidiana, contexto y revisiones previstas en una sola línea de tiempo.</p></div><div class="heading-actions"><button class="secondary-button" @click="showSchedule=!showSchedule">Gestionar horarios</button><div class="calendar-modes"><button :class="{ active: mode === 'week' }" @click="changeMode('week')">Semana</button><button :class="{ active: mode === 'month' }" @click="changeMode('month')">Mes</button></div></div></div>
    <div v-if="error" class="notice error">{{ error }}</div><div v-if="success" class="notice atlas-success">{{ success }}</div>
    <article v-if="showSchedule" class="panel schedule-manager"><div class="panel-heading"><div><span class="eyebrow">Rutina semanal</span><h2>Horarios recurrentes</h2><p>Registrá compromisos que se repiten para verlos junto al entrenamiento y detectar cruces horarios.</p></div><button class="secondary-button" :disabled="savingSchedule" @click="loadPersonalSchedule">Cargar mi horario acordado</button></div><form @submit.prevent="saveBlock"><div class="atlas-form-grid"><label>Nombre<input v-model="scheduleForm.name" required placeholder="Ej.: Trabajo"></label><label>Tipo<select v-model="scheduleForm.category"><option value="work">Trabajo</option><option value="training">Entrenamiento</option><option value="recovery">Recuperación</option><option value="personal">Personal</option></select></label><label>Franja<select v-model="scheduleForm.timeWindow"><option value="exact">Horario exacto</option><option value="morning">Por la mañana</option><option value="afternoon">Por la tarde</option><option value="evening">Por la noche</option><option value="flexible">Flexible</option></select></label><template v-if="scheduleForm.timeWindow==='exact'"><label>Desde<input v-model="scheduleForm.startTime" type="time" required></label><label>Hasta<input v-model="scheduleForm.endTime" type="time" required></label></template><label>Vigente desde<input v-model="scheduleForm.effectiveFrom" type="date" required></label><label>Vigente hasta<input v-model="scheduleForm.effectiveTo" type="date"></label><label class="wide schedule-days"><span>Días</span><i v-for="n in 7" :key="n"><input v-model="scheduleForm.daysOfWeek" type="checkbox" :value="n">{{dayNames[n]}}</i></label><label class="wide">Notas<input v-model="scheduleForm.notes"></label></div><button class="primary-button" :disabled="savingSchedule">{{savingSchedule?'Guardando…':'Agregar horario'}}</button></form><div class="schedule-list"><div v-for="block in schedule" :key="block.id"><span :class="block.category"></span><strong>{{dayNames[block.dayOfWeek]}} · {{block.name}}</strong><small>{{windowLabel(block)}} · desde {{fromIso(block.effectiveFrom).toLocaleDateString('es-AR')}}</small><button class="small-button" @click="removeBlock(block.id)">Quitar</button></div><p v-if="!schedule.length" class="list-empty">Todavía no hay horarios recurrentes.</p></div></article>
    <div v-if="data" class="calendar-summary">
      <article><span>Sesiones</span><strong>{{ data.summary.plannedSessions }}</strong></article><article><span>Completadas</span><strong>{{ data.summary.completedSessions }}</strong></article><article><span>Revisiones previstas</span><strong>{{ data.summary.scheduledLearningReviews }}</strong></article><article :class="{attention:data.summary.dueLearningReviews}"><span>Para revisar</span><strong>{{ data.summary.dueLearningReviews }}</strong></article><article><span>Minutos reales</span><strong>{{ data.summary.actualMinutes }}</strong></article><article><span>Carga entrenamiento</span><strong>{{ data.summary.internalLoad }}</strong></article><article><span>Carga externa</span><strong>{{ data.summary.externalLoad }}</strong></article><article><span>Carga total</span><strong>{{ data.summary.totalLoad }}</strong></article>
    </div>
    <article v-if="data" class="panel calendar-panel">
      <div class="calendar-toolbar"><button @click="move(-1)">‹</button><h2>{{ title }}</h2><button @click="move(1)">›</button></div>
      <div class="calendar-weekdays"><span v-for="day in weekdays" :key="day">{{ day }}</span></div>
      <div class="calendar-grid" :class="mode">
        <button v-for="day in data.days" :key="day.date" class="calendar-day" :class="{ selected: day.date === selectedDate, today: day.date === today, outside: outside(day.date), hasSessions: day.sessions.length || day.learningReviews.length || day.scheduleBlocks.length, conflict:day.hasScheduleConflict }" @click="selectedDate = day.date">
          <span class="day-number">{{ fromIso(day.date).getDate() }}</span><span v-if="day.hasCheckIn" class="check-dot" title="Check-in registrado"></span>
          <div class="day-sessions"><i v-for="block in day.scheduleBlocks.slice(0,mode==='week'?4:2)" :key="block.id" class="schedule-block">{{clock(block.startTime)||windowLabel(block)}} · {{block.name}}</i><i v-for="session in day.sessions.slice(0, mode === 'week' ? 4 : 2)" :key="session.id" :class="session.status.toLowerCase()"><template v-if="session.plannedStartTime">{{clock(session.plannedStartTime)}} · </template>{{ session.activityType }}<b v-if="session.followUpPending">!</b></i><i v-for="review in day.learningReviews.slice(0,mode==='week'?2:1)" :key="review.id" class="learning-review" :class="{due:review.isDue,reviewed:review.status==='applied'}">Bitácora<b v-if="review.isDue">!</b><b v-else-if="review.status==='applied'">✓</b></i><small v-if="day.dailyActivities">+ {{ day.dailyActivities }} actividad{{ day.dailyActivities === 1 ? '' : 'es' }} cotidiana{{ day.dailyActivities === 1 ? '' : 's' }}</small></div>
          <span v-if="day.totalLoad" class="day-load">Total {{ day.totalLoad }} UA</span>
        </button>
      </div>
    </article>
    <div v-if="selected" class="calendar-detail">
      <article class="panel"><div class="panel-heading"><div><span class="eyebrow">{{ fromIso(selected.date).toLocaleDateString('es-AR', { weekday: 'long', day: '2-digit', month: 'long' }) }}</span><h2>Detalle del día</h2></div><div class="heading-actions"><button class="secondary-button" @click="prepareOneOff">Agregar compromiso puntual</button><button class="primary-button" @click="openPlan(selected)">Planificar sesión</button></div></div>
        <div class="day-context"><span :class="{ complete: selected.hasCheckIn }">{{ selected.hasCheckIn ? '✓ Check-in' : 'Sin check-in' }}</span><span>Energía: {{ selected.energy ?? '—' }}</span><span>Fatiga: {{ selected.fatigue ?? '—' }}</span><span>Dolor: {{ selected.pain ?? '—' }}</span><span>Entrenamiento: {{ selected.internalLoad }} UA</span><span>Externa: {{ selected.externalLoad }} UA</span><span>Total: {{ selected.totalLoad }} UA</span></div>
        <div v-if="selected.scheduleBlocks.length" class="day-schedule"><span class="eyebrow">Horarios recurrentes</span><div v-for="block in selected.scheduleBlocks" :key="block.id"><span><strong>{{windowLabel(block)}} · {{block.name}}</strong><small>{{block.notes}}</small></span><button v-if="block.category==='training'" class="small-button" @click="openPlanFromBlock(block)">Preparar sesión</button></div><p v-if="selected.hasScheduleConflict" class="schedule-conflict">Hay bloques con horarios superpuestos. Revisá si el cruce es intencional.</p></div>
        <div class="agenda-sessions"><RouterLink v-for="session in selected.sessions" :key="session.id" to="/entrenamiento"><div><strong>{{ session.name }}</strong><small><template v-if="session.plannedStartTime">{{clock(session.plannedStartTime)}} · </template>{{ session.activityType }} · {{ session.status === 'Completed' ? 'Completada' : session.status === 'InProgress' ? 'En curso' : 'Planificada' }}</small></div><span>{{ session.actualDurationMinutes ?? session.plannedDurationMinutes ?? '—' }} min · RPE {{ session.sessionRpe ?? session.targetRpe ?? '—' }}</span><em v-if="session.followUpPending">24 h pendiente</em></RouterLink><p v-if="!selected.sessions.length" class="list-empty">No hay sesiones formales planificadas para este día.</p><RouterLink v-if="selected.dailyActivities" to="/#carga-diaria" class="daily-load-link">{{ selected.dailyActivities }} actividad{{ selected.dailyActivities === 1 ? '' : 'es' }} cotidiana{{ selected.dailyActivities === 1 ? '' : 's' }} registrada{{ selected.dailyActivities === 1 ? '' : 's' }} · {{ selected.externalLoad }} UA</RouterLink></div>
        <div v-if="selected.learningReviews.length" class="agenda-reminders"><span class="eyebrow">Revisiones de bitácora</span><RouterLink v-for="review in selected.learningReviews" :key="review.id" :to="{path:'/bitacora',query:{entry:review.id}}" :class="{due:review.isDue,reviewed:review.status==='applied'}"><div><strong>{{review.title}}</strong><small>{{review.nextAction}}</small></div><em>{{review.status==='applied'?'Revisada':review.isDue?'Para revisar':'Programada'}}</em></RouterLink></div>
      </article>
      <form v-if="showPlan" class="panel quick-plan" @submit.prevent="plan"><div class="panel-heading"><div><span class="eyebrow">Planificación rápida</span><h2>Nueva sesión</h2></div><button type="button" class="icon-button" @click="showPlan = false">×</button></div><div class="atlas-form-grid"><label>Fecha<input v-model="form.date" type="date" required></label><label>Hora prevista<input v-model="form.plannedStartTime" type="time"></label><label>Nombre<input v-model="form.name" required></label><label>Actividad<select v-model="form.activityType"><option>Fuerza</option><option>Cardio</option><option>Taekwondo</option><option>Movilidad</option><option>Recuperación</option><option>Otra</option></select></label><label>Duración<input v-model.number="form.plannedDurationMinutes" type="number" min="1"></label><label>RPE objetivo<select v-model.number="form.targetRpe"><option v-for="n in 10" :key="n" :value="n">{{ n }} / 10</option></select></label><label>Objetivo<input v-model="form.goal"></label></div><label>Notas<textarea v-model="form.notes" rows="2"></textarea></label><button class="primary-button">Agregar a la agenda</button></form>
    </div>
  </section>
</template>
