<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { deleteLearning, getLearning, saveLearning } from '../services/learningService'
import type { LearningEntry, LearningOutcome, LearningOverview } from '../types/learning'

const data=ref<LearningOverview|null>(null), editing=ref<string|null>(null), showForm=ref(false)
const filter=ref('all'), error=ref(''), success=ref(''), prefilled=ref(false)
const route=useRoute(), today=new Date().toLocaleDateString('en-CA')
const blank=()=>({date:today,title:'',category:'Entrenamiento',observation:'',interpretation:'',nextAction:'',reviewDueOn:null as string|null,confidence:'low',status:'open',reviewedOn:null as string|null,followUpOutcome:'' as LearningOutcome|'',followUpObservation:'',trainingSessionId:null as string|null,personalGoalId:null as string|null,trainingCycleId:null as string|null,changeReason:'Creación inicial de la entrada'})
const form=reactive(blank())
const visible=computed(()=>data.value?.entries.filter(x=>filter.value==='all'||x.status===filter.value)??[])

async function load(){try{data.value=await getLearning()}catch(e){error.value=e instanceof Error?e.message:'No se pudo abrir la bitácora.'}}
function reset(){editing.value=null;showForm.value=false;prefilled.value=false;Object.assign(form,blank())}
function edit(x:LearningEntry){editing.value=x.id;prefilled.value=false;Object.assign(form,{date:x.date,title:x.title,category:x.category,observation:x.observation,interpretation:x.interpretation??'',nextAction:x.nextAction??'',reviewDueOn:x.reviewDueOn,confidence:x.confidence,status:x.status,reviewedOn:x.reviewedOn,followUpOutcome:x.followUpOutcome??'',followUpObservation:x.followUpObservation??'',trainingSessionId:x.trainingSessionId,personalGoalId:x.personalGoalId,trainingCycleId:x.trainingCycleId,changeReason:''});showForm.value=true;scrollTo({top:0,behavior:'smooth'})}
function review(x:LearningEntry){edit(x);form.status='applied';form.reviewedOn=x.reviewedOn??today;form.changeReason='Seguimiento de la próxima acción'}
async function submit(){error.value='';try{await saveLearning(form,editing.value??undefined);success.value='Aprendizaje guardado y versionado.';reset();await load()}catch(e){error.value=e instanceof Error?e.message:'No se pudo guardar.'}}
async function remove(id:string){try{await deleteLearning(id);await load()}catch(e){error.value=e instanceof Error?e.message:'No se pudo eliminar.'}}
const fmt=(v:string)=>new Date(`${v}T12:00:00`).toLocaleDateString('es-AR')
const confidence=(v:string)=>v==='high'?'Alta':v==='medium'?'Media':'Baja'
const outcome=(v:LearningOutcome|null)=>v==='helpful'?'Acompañó una mejora':v==='neutral'?'Sin cambio claro':v==='not-helpful'?'No resultó útil':'Resultado no concluyente'
const isDue=(v:string|null)=>Boolean(v&&v<=today)

onMounted(async()=>{await load();if(['all','open','applied','archived'].includes(String(route.query.filter)))filter.value=String(route.query.filter);const requested=data.value?.entries.find(x=>x.id===String(route.query.entry));if(requested){edit(requested);return}if(route.query.source==='decision'||route.query.source==='weekly'){const date=String(route.query.date??today),weekly=route.query.source==='weekly';Object.assign(form,{date,title:String(route.query.title??(weekly?'Revisión semanal':'Revisión de decisión')),category:String(route.query.category??(weekly?'Revisión semanal':'Entrenamiento')),observation:String(route.query.observation??''),interpretation:'',nextAction:'',reviewDueOn:null,confidence:'low',status:'open',reviewedOn:null,followUpOutcome:'',followUpObservation:'',trainingSessionId:weekly?null:data.value?.sessions.find(x=>x.detail.startsWith(date))?.id??null,changeReason:weekly?'Reflexión creada desde el resumen semanal':'Reflexión creada desde la cronología de decisiones'});prefilled.value=true;showForm.value=true}})
</script>

<template>
  <section class="page learning-page">
    <div class="page-heading"><div><span class="eyebrow">Aprendizaje personal</span><h1>Bitácora Atlas</h1><p>Convertí experiencias en observaciones trazables, hipótesis prudentes y próximas acciones.</p></div><button class="primary-button" @click="showForm=true">Nueva entrada</button></div>
    <div v-if="error" class="notice error">{{error}}</div><div v-if="success" class="notice atlas-success">{{success}}</div>
    <div v-if="data" class="learning-summary"><article><span>Últimos 30 días</span><strong>{{data.entriesLast30Days}}</strong></article><article><span>Acciones abiertas</span><strong>{{data.openActions}}</strong></article><article><span>Con seguimiento</span><strong>{{data.reviewedActions}}</strong></article><article><span>Total de entradas</span><strong>{{data.entries.length}}</strong></article></div>
    <form v-if="showForm" class="panel learning-form" @submit.prevent="submit">
      <div class="panel-heading"><div><span class="eyebrow">{{editing?'Nueva versión':'Reflexión estructurada'}}</span><h2>{{editing?'Corregir o revisar entrada':'Registrar aprendizaje'}}</h2></div><button type="button" class="icon-button" @click="reset">×</button></div>
      <div v-if="prefilled" class="learning-prefill-note"><strong>Borrador creado desde Tendencias</strong><p>Atlas completó sólo observaciones descriptivas. Revisalas y escribí tu propia interpretación y próxima acción.</p></div>
      <div class="atlas-form-grid">
        <label>Fecha<input v-model="form.date" type="date" required></label><label>Título<input v-model="form.title" required></label>
        <label>Categoría<select v-model="form.category"><option>Entrenamiento</option><option>Técnica</option><option>Recuperación</option><option>Rodilla</option><option>Hábitos</option><option>Trabajo</option><option>Planificación</option><option>Revisión semanal</option></select></label>
        <label>Confianza en la interpretación<select v-model="form.confidence"><option value="low">Baja</option><option value="medium">Media</option><option value="high">Alta</option></select></label>
        <label>Estado<select v-model="form.status"><option value="open">Acción abierta</option><option value="applied">Aplicada y revisada</option><option value="archived">Archivada</option></select></label>
        <label>Sesión vinculada<select v-model="form.trainingSessionId"><option :value="null">Sin sesión</option><option v-for="x in data?.sessions" :key="x.id" :value="x.id">{{x.name}} · {{x.detail}}</option></select></label>
        <label>Objetivo vinculado<select v-model="form.personalGoalId"><option :value="null">Sin objetivo</option><option v-for="x in data?.goals" :key="x.id" :value="x.id">{{x.name}}</option></select></label>
        <label>Ciclo vinculado<select v-model="form.trainingCycleId"><option :value="null">Sin ciclo</option><option v-for="x in data?.cycles" :key="x.id" :value="x.id">{{x.name}}</option></select></label>
        <label class="wide learning-observation">Observación comprobable<textarea v-model="form.observation" rows="3" required placeholder="Qué ocurrió, sin explicar todavía por qué"></textarea></label>
        <label class="wide learning-hypothesis">Interpretación provisional<textarea v-model="form.interpretation" rows="3" placeholder="Qué podría significar; no es un diagnóstico"></textarea></label>
        <label class="wide learning-action">Próxima acción<textarea v-model="form.nextAction" rows="2" placeholder="Qué vas a repetir, cambiar, medir o consultar"></textarea></label>
        <label>Revisión prevista (opcional)<input v-model="form.reviewDueOn" type="date" :min="form.date"><small>Atlas la mostrará en Hoy cuando llegue esta fecha.</small></label>
        <template v-if="form.status==='applied'||form.reviewedOn">
          <div class="wide learning-review-note"><strong>Seguimiento de la acción</strong><p>Registrá sólo lo que observaste. Este resultado no demuestra por sí solo una relación de causa y efecto.</p></div>
          <label>Fecha de revisión<input v-model="form.reviewedOn" type="date" :min="form.date" required></label>
          <label>Resultado observado<select v-model="form.followUpOutcome" required><option value="" disabled>Seleccionar</option><option value="helpful">Acompañó una mejora</option><option value="neutral">Sin cambio claro</option><option value="not-helpful">No resultó útil</option><option value="inconclusive">No concluyente</option></select></label>
          <label class="wide learning-followup">Observación de seguimiento<textarea v-model="form.followUpObservation" rows="3" required placeholder="Qué pasó después de aplicar la acción"></textarea></label>
        </template>
        <label class="wide">Motivo de esta versión<input v-model="form.changeReason" required></label>
      </div><button class="primary-button">Guardar entrada</button>
    </form>
    <div class="learning-filters"><button :class="{active:filter==='all'}" @click="filter='all'">Todas</button><button :class="{active:filter==='open'}" @click="filter='open'">Abiertas</button><button :class="{active:filter==='applied'}" @click="filter='applied'">Aplicadas</button><button :class="{active:filter==='archived'}" @click="filter='archived'">Archivadas</button></div>
    <div class="learning-list"><article v-for="x in visible" :key="x.id" class="panel learning-entry"><div class="learning-entry-head"><div><span class="fact-status" :class="x.status">{{x.status==='open'?'Abierta':x.status==='applied'?'Aplicada y revisada':'Archivada'}}</span><small>{{fmt(x.date)}} · {{x.category}} · v{{x.version}}</small></div><em :class="x.confidence">Confianza {{confidence(x.confidence)}}</em></div><h2>{{x.title}}</h2><div class="learning-layers"><section><span>01 · Observación</span><p>{{x.observation}}</p></section><section v-if="x.interpretation"><span>02 · Interpretación provisional</span><p>{{x.interpretation}}</p></section><section v-if="x.nextAction"><span>03 · Próxima acción</span><p>{{x.nextAction}}</p><small v-if="x.reviewDueOn" class="learning-due" :class="{overdue:x.status==='open'&&isDue(x.reviewDueOn)}">{{x.status==='open'&&isDue(x.reviewDueOn)?'Lista para revisar':'Revisión prevista'}} · {{fmt(x.reviewDueOn)}}</small></section></div><section v-if="x.reviewedOn&&x.followUpOutcome&&x.followUpObservation" class="learning-outcome"><div><span>04 · Seguimiento · {{fmt(x.reviewedOn)}}</span><strong>{{outcome(x.followUpOutcome)}}</strong></div><p>{{x.followUpObservation}}</p><small>Resultado descriptivo; no prueba causalidad.</small></section><div class="learning-links"><span v-if="x.trainingSessionName">Sesión · {{x.trainingSessionName}}</span><span v-if="x.personalGoalName">Objetivo · {{x.personalGoalName}}</span><span v-if="x.trainingCycleName">Ciclo · {{x.trainingCycleName}}</span></div><div class="row-actions"><button v-if="x.status==='open'&&x.nextAction" class="small-button followup" @click="review(x)">Registrar seguimiento</button><button class="small-button" @click="edit(x)">Editar</button><button class="small-button danger" @click="remove(x.id)">Eliminar</button></div></article><p v-if="!visible.length" class="panel list-empty">No hay entradas para este filtro.</p></div>
  </section>
</template>
