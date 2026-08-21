<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { createReportShare, getProfessionalReport, getSharedReport, listReportFeedback, listReportShares, reviewReportFeedback, revokeReportShare, submitReportFeedback } from '../services/reportService'
import type { ProfessionalReport, ReportFeedback, ReportShare } from '../types/report'

const route = useRoute()
const publicToken = computed(() => typeof route.params.token === 'string' ? route.params.token : '')
const data = ref<ProfessionalReport | null>(null)
const shares = ref<ReportShare[]>([])
const feedback = ref<ReportFeedback[]>([])
const weeks = ref(8)
const loading = ref(true)
const error = ref('')
const success = ref('')
const showKnee = ref(true)
const showLearning = ref(true)
const shareOpen = ref(false)
const consent = ref(false)
const recipientLabel = ref('')
const expiresInDays = ref(7)
const createdLink = ref('')
const sharedExpiry = ref('')
const feedbackAuthor = ref('')
const feedbackKind = ref<'comment'|'proposal'>('comment')
const feedbackSection = ref<'general'|'activity'|'goals'|'knee'|'learning'>('general')
const feedbackMessage = ref('')
const feedbackSent = ref(false)
const decisionNotes = ref<Record<string,string>>({})
const maxLoad = computed(() => Math.max(1, ...(data.value?.weeks.map(x => x.totalLoad) ?? [])))

const toIso = () => new Date().toLocaleDateString('en-CA')
function fromIso() { const d = new Date(); d.setDate(d.getDate() - (weeks.value * 7 - 1)); return d.toLocaleDateString('en-CA') }
const date = (v: string) => new Date(`${v}T12:00:00`).toLocaleDateString('es-AR', { day: '2-digit', month: 'short', year: 'numeric' })
const value = (v: number | null, suffix = '') => v === null ? '—' : `${v}${suffix}`
const hours = (minutes: number) => Math.round(minutes / 60 * 10) / 10
const statusLabel = (status: ReportShare['status']) => status === 'active' ? 'Activo' : status === 'expired' ? 'Vencido' : 'Revocado'
const feedbackStatus = (status: ReportFeedback['status']) => ({pending:'Pendiente',reviewed:'Revisado',incorporated:'Incorporado manualmente',dismissed:'Descartado'})[status]
const sectionLabel = (section: ReportFeedback['section']) => ({general:'General',activity:'Actividad y carga',goals:'Objetivos',knee:'Rodilla',learning:'Bitácora'})[section]

async function load() {
  loading.value = true; error.value = ''
  try { data.value = await getProfessionalReport(fromIso(), toIso()) }
  catch (e) { error.value = e instanceof Error ? e.message : 'No se pudo generar.' }
  finally { loading.value = false }
}
async function loadShares() { try { shares.value = await listReportShares() } catch { shares.value = [] } }
async function loadFeedback() { try { feedback.value = await listReportFeedback() } catch { feedback.value = [] } }
async function loadShared() {
  loading.value = true; error.value = ''
  try {
    const shared = await getSharedReport(publicToken.value)
    data.value = shared.report; sharedExpiry.value = shared.expiresAt
    showKnee.value = shared.report.kneeChecks.length > 0
    showLearning.value = shared.report.learning.length > 0
  } catch (e) { error.value = e instanceof Error ? e.message : 'No se pudo abrir el informe.' }
  finally { loading.value = false }
}
async function createShare() {
  if (!data.value) return
  error.value = ''; success.value = ''; createdLink.value = ''
  try {
    const created = await createReportShare({ from: data.value.from, to: data.value.to, expiresInDays: expiresInDays.value, includeKnee: showKnee.value, includeLearning: showLearning.value, recipientLabel: recipientLabel.value, consent: consent.value })
    createdLink.value = `${window.location.origin}/informe-compartido/${created.token}`
    success.value = 'Enlace privado creado. Guardalo ahora: el código completo no vuelve a mostrarse.'
    consent.value = false
    await loadShares()
  } catch (e) { error.value = e instanceof Error ? e.message : 'No se pudo compartir.' }
}
async function copyLink() {
  try { await navigator.clipboard.writeText(createdLink.value); success.value = 'Enlace copiado.' }
  catch { success.value = 'Seleccioná y copiá el enlace manualmente.' }
}
async function revoke(id: string) { await revokeReportShare(id); await loadShares(); success.value = 'Acceso revocado.' }
async function sendFeedback() {
  error.value = ''
  try { await submitReportFeedback(publicToken.value,{authorName:feedbackAuthor.value,kind:feedbackKind.value,section:feedbackSection.value,message:feedbackMessage.value});feedbackSent.value=true;feedbackMessage.value='' }
  catch(e){error.value=e instanceof Error?e.message:'No se pudo enviar el aporte.'}
}
async function reviewFeedback(id:string,status:'reviewed'|'incorporated'|'dismissed') {
  error.value=''
  try { await reviewReportFeedback(id,{status,decisionNote:decisionNotes.value[id]??''});await loadFeedback();success.value='Decisión registrada sin modificar automáticamente tu plan.' }
  catch(e){error.value=e instanceof Error?e.message:'No se pudo registrar la decisión.'}
}
function print() { window.print() }

onMounted(async () => publicToken.value ? loadShared() : Promise.all([load(), loadShares(), loadFeedback()]))
</script>

<template>
  <section class="page professional-report-page" :class="{ muted: loading, 'shared-report-page': publicToken }">
    <div class="page-heading professional-report-controls">
      <div><span class="eyebrow">{{ publicToken ? 'Copia privada de solo lectura' : 'Documento compartible' }}</span><h1>Informe profesional</h1><p>Resumen descriptivo para revisión conjunta con profesionales.</p></div>
      <div class="report-actions">
        <div v-if="!publicToken" class="evaluation-period"><button v-for="n in [4,8,12]" :key="n" :class="{active:weeks===n}" @click="weeks=n;load()">{{n}} sem.</button></div>
        <button v-if="!publicToken" class="secondary-button" @click="shareOpen=!shareOpen">Compartir acceso</button>
        <button class="primary-button print-button" @click="print">Imprimir / Guardar PDF</button>
      </div>
    </div>

    <div v-if="!publicToken" class="report-section-toggles"><label><input v-model="showKnee" type="checkbox"> Incluir rodilla</label><label><input v-model="showLearning" type="checkbox"> Incluir bitácora</label></div>
    <div v-if="error" class="notice error">{{error}}</div>
    <div v-if="success" class="notice atlas-success">{{success}}</div>
    <div v-if="publicToken && data" class="notice shared-notice">Documento congelado · acceso de solo lectura hasta {{new Date(sharedExpiry).toLocaleString('es-AR')}}</div>

    <section v-if="shareOpen && !publicToken" class="panel report-share-panel professional-report-controls">
      <div class="panel-heading"><div><span class="eyebrow">Consentimiento y acceso</span><h2>Crear enlace privado</h2><p>Comparte esta versión exacta; no permite navegar tu cuenta ni verá cambios posteriores.</p></div></div>
      <div class="report-share-form"><label>Destinatario o motivo<input v-model="recipientLabel" maxlength="160" placeholder="Ej.: kinesiólogo — revisión de agosto"></label><label>Vencimiento<select v-model="expiresInDays"><option :value="1">24 horas</option><option :value="7">7 días</option><option :value="14">14 días</option><option :value="30">30 días</option></select></label></div>
      <label class="share-consent"><input v-model="consent" type="checkbox"> Confirmo que deseo compartir los apartados seleccionados durante este plazo.</label>
      <button class="primary-button" :disabled="!consent" @click="createShare">Generar enlace privado</button>
      <div v-if="createdLink" class="created-share-link"><input :value="createdLink" readonly><button class="secondary-button" @click="copyLink">Copiar</button></div>
      <div v-if="shares.length" class="share-history"><h3>Accesos recientes</h3><article v-for="item in shares" :key="item.id"><div><strong>{{item.recipientLabel||'Sin destinatario indicado'}}</strong><small>{{date(item.from)}} — {{date(item.to)}} · vence {{new Date(item.expiresAt).toLocaleString('es-AR')}}</small></div><span :class="item.status">{{statusLabel(item.status)}}</span><button v-if="item.status==='active'" class="text-button danger" @click="revoke(item.id)">Revocar</button></article></div>
    </section>

    <section v-if="!publicToken && feedback.length" class="panel report-feedback-inbox professional-report-controls">
      <div class="panel-heading"><div><span class="eyebrow">Revisión personal</span><h2>Aportes profesionales</h2><p>Ningún aporte modifica Atlas automáticamente. Vos registrás la decisión final.</p></div><span class="feedback-count">{{feedback.filter(x=>x.status==='pending').length}} pendientes</span></div>
      <article v-for="item in feedback" :key="item.id" class="feedback-card" :class="item.status">
        <header><div><strong>{{item.authorName}}</strong><small>{{item.kind==='proposal'?'Propuesta':'Observación'}} · {{sectionLabel(item.section)}} · {{new Date(item.createdAt).toLocaleString('es-AR')}}</small></div><span>{{feedbackStatus(item.status)}}</span></header>
        <p>{{item.message}}</p><small>Informe {{date(item.reportFrom)}} — {{date(item.reportTo)}} · {{item.shareLabel||'sin destinatario indicado'}}</small>
        <div v-if="item.status==='pending'" class="feedback-decision"><input v-model="decisionNotes[item.id]" placeholder="Nota de decisión opcional"><button class="secondary-button" @click="reviewFeedback(item.id,'reviewed')">Marcar revisado</button><button class="secondary-button" @click="reviewFeedback(item.id,'incorporated')">Incorporado</button><button class="text-button danger" @click="reviewFeedback(item.id,'dismissed')">Descartar</button></div>
        <p v-else-if="item.decisionNote" class="decision-note"><b>Tu decisión:</b> {{item.decisionNote}}</p>
      </article>
    </section>

    <article v-if="data" class="professional-document">
      <header><div><span>PROYECTO ATLAS · INFORME PERSONAL</span><h1>{{data.profile.displayName}}</h1><p>Período {{date(data.from)}} — {{date(data.to)}}</p></div><div><strong>Generado</strong><span>{{new Date(data.generatedAt).toLocaleString('es-AR')}}</span><strong>Cobertura</strong><span>{{data.dataCoverageDays}} días con datos</span></div></header>
      <section class="report-disclaimer">{{data.disclaimer}}</section>
      <section class="report-profile"><div><span>Objetivo principal</span><strong>{{data.profile.primaryGoal??'Sin definir'}}</strong></div><div><span>Referencia</span><strong>{{value(data.profile.heightCm,' cm')}} · {{value(data.profile.referenceWeightKg,' kg')}}</strong></div><div><span>Rodilla afectada</span><strong>{{data.profile.affectedKnee??'Sin definir'}}</strong></div></section>
      <section class="report-block"><div class="report-title"><span>01</span><div><h2>Actividad y contexto</h2><p>Datos registrados durante el período seleccionado.</p></div></div><div class="report-kpis"><article><span>Carga total</span><strong>{{data.summary.totalLoad}} UA</strong><small>{{data.summary.trainingLoad}} entrenamiento + {{data.summary.externalLoad}} externa</small></article><article><span>Sesiones</span><strong>{{data.summary.sessions}}</strong><small>{{data.summary.trainingMinutes}} minutos</small></article><article><span>Actividad cotidiana</span><strong>{{data.summary.externalMinutes}} min</strong><small>Trabajo, bicicleta y otras</small></article><article><span>Check-ins</span><strong>{{data.summary.checkIns}}</strong><small>durante el período</small></article></div><div v-if="data.work" class="report-work-context"><header><div><strong>Contexto laboral</strong><small>{{data.work.recordedDays}} jornadas comparables · {{data.work.weeksWithData}} semanas con datos<br>{{data.work.contextRecordedDays}} detalladas · {{data.work.breakMinutes}} min de pausas · {{data.work.unusualDays}} días inusuales</small></div><span>Previsto <b>{{hours(data.work.plannedMinutes)}} h</b></span><span>Realizado <b>{{hours(data.work.actualMinutes)}} h</b></span><span>Adicional <b>{{hours(data.work.extraMinutes)}} h</b></span><span>Diferencia neta <b>{{data.work.differenceMinutes>0?'+':''}}{{data.work.differenceMinutes}} min</b></span></header><div v-if="data.work.recordedDays" class="report-work-weeks"><span v-for="(item,index) in data.weeks" :key="item.from" :class="{empty:!item.workRecordedDays}"><b>S{{index+1}}</b><template v-if="item.workRecordedDays">{{hours(item.workActualMinutes??0)}} / {{hours(item.workPlannedMinutes??0)}} h<small>+{{item.workExtraMinutes??0}} min adicionales</small><small v-if="item.workContextRecordedDays">{{item.workBreakMinutes??0}} min pausas · {{item.workUnusualDays??0}} inusuales</small></template><template v-else>Sin comparación</template></span></div><p>Duración real / prevista. Las pausas son informadas, no una medición de recuperación. Este contexto no establece por sí solo la causa de cambios físicos o de rendimiento.</p></div><div class="report-week-chart"><div v-for="(item,index) in data.weeks" :key="item.from"><span>{{item.totalLoad||''}}</span><div><i class="external" :style="{height:`${item.externalLoad/maxLoad*100}%`}"></i><i class="training" :style="{height:`${item.trainingLoad/maxLoad*100}%`}"></i></div><small>S{{index+1}}</small></div></div><div class="report-wellbeing"><span>Sueño <strong>{{value(data.summary.averageSleepQuality,' / 5')}}</strong></span><span>Energía <strong>{{value(data.summary.averageEnergy,' / 5')}}</strong></span><span>Fatiga <strong>{{value(data.summary.averageFatigue,' / 10')}}</strong></span><span>Estrés <strong>{{value(data.summary.averageStress,' / 5')}}</strong></span><span>Dolor <strong>{{value(data.summary.averagePain,' / 10')}}</strong></span></div></section>
      <section class="report-block"><div class="report-title"><span>02</span><div><h2>Objetivos e indicadores</h2><p>Progreso calculado únicamente cuando existen mediciones comparables.</p></div></div><div class="report-goals"><article v-for="goal in data.goals" :key="goal.title"><div><strong>{{goal.title}}</strong><small>{{goal.category}} · {{goal.status}}</small></div><span>{{goal.baselineValue??'—'}} → {{goal.latestValue??'—'}} → {{goal.targetValue??'—'}} {{goal.unit}}</span><em>{{goal.progressPercent===null?'Sin progreso calculable':`${goal.progressPercent}%`}}</em></article></div><div class="report-metrics"><article v-for="metric in data.metrics" :key="metric.name"><strong>{{metric.name}}</strong><span>{{metric.firstValue}} → {{metric.latestValue}} {{metric.unit}}</span><small>{{date(metric.firstDate)}} — {{date(metric.latestDate)}} · {{metric.entries}} registros</small></article><p v-if="!data.metrics.length">Sin indicadores con dos registros en este período.</p></div></section>
      <section v-if="showKnee" class="report-block"><div class="report-title"><span>03</span><div><h2>Respuesta funcional de rodilla</h2><p>Autorreporte; no corresponde a una evaluación clínica.</p></div></div><div class="report-knee"><article v-for="item in data.kneeChecks" :key="item.recordedAt" :class="item.state"><div><strong>{{new Date(item.recordedAt).toLocaleString('es-AR')}}</strong><small>{{item.context}} · {{item.side}}</small></div><span>Dolor {{item.painNow}}/10 · máximo {{item.painWorst24H}}/10 · función {{item.function}}/10</span><p>{{item.reasons.join(' · ')||'Sin señales clasificadas de atención.'}}</p></article><p v-if="!data.kneeChecks.length">Sin controles de rodilla en el período.</p></div></section>
      <section v-if="showLearning" class="report-block"><div class="report-title"><span>04</span><div><h2>Observaciones y próximas acciones</h2><p>Entradas personales de la Bitácora Atlas.</p></div></div><div class="report-learning"><article v-for="item in data.learning" :key="`${item.date}-${item.title}`"><span>{{date(item.date)}} · confianza {{item.confidence}}</span><strong>{{item.title}}</strong><p><b>Observación:</b> {{item.observation}}</p><p v-if="item.interpretation"><b>Interpretación provisional:</b> {{item.interpretation}}</p><p v-if="item.nextAction"><b>Próxima acción:</b> {{item.nextAction}}</p></article><p v-if="!data.learning.length">Sin entradas de bitácora en el período.</p></div></section>
      <footer>Proyecto Atlas · Documento generado desde registros personales · {{new Date(data.generatedAt).toLocaleDateString('es-AR')}}</footer>
    </article>

    <section v-if="publicToken && data" class="panel public-feedback-panel professional-report-controls">
      <div v-if="feedbackSent" class="feedback-thanks"><strong>Aporte enviado</strong><p>Jonathan podrá revisarlo y registrar su decisión. El plan no fue modificado automáticamente.</p></div>
      <template v-else><div class="panel-heading"><div><span class="eyebrow">Canal de colaboración</span><h2>Dejar una observación o propuesta</h2><p>El aporte quedará asociado a esta copia del informe y será revisado por su titular.</p></div></div>
      <div class="public-feedback-form"><label>Nombre<input v-model="feedbackAuthor" maxlength="120" placeholder="Nombre del profesional"></label><label>Tipo<select v-model="feedbackKind"><option value="comment">Observación</option><option value="proposal">Propuesta</option></select></label><label>Sección<select v-model="feedbackSection"><option value="general">General</option><option value="activity">Actividad y carga</option><option value="goals">Objetivos</option><option v-if="showKnee" value="knee">Rodilla</option><option v-if="showLearning" value="learning">Bitácora</option></select></label><label class="wide">Mensaje<textarea v-model="feedbackMessage" maxlength="1600" rows="5" placeholder="Describa la observación y su fundamento. No incluya información innecesariamente sensible."></textarea></label></div>
      <button class="primary-button" :disabled="!feedbackAuthor.trim()||!feedbackMessage.trim()" @click="sendFeedback">Enviar para revisión</button></template>
    </section>
  </section>
</template>
