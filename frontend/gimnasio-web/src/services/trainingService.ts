import type { CopyTrainingWeekResult, ScheduleBlock, TrainingCalendar, TrainingFollowUp, TrainingOverview, TrainingSession } from '../types/training'
import { queueOperation } from './offlineQueue'
async function request<T>(url:string, options?:RequestInit):Promise<T>{ const response=await fetch(url,{...options,credentials:'include'}); if(!response.ok){const body=await response.json().catch(()=>null) as {message?:string}|null; throw new Error(body?.message??'No se pudo completar la operación.')} return response.json() as Promise<T> }
const json=(body:unknown,method='POST'):RequestInit=>({method,headers:{'Content-Type':'application/json'},body:JSON.stringify(body)})
export const getTrainingOverview=()=>request<TrainingOverview>('/api/training')
export const createSession=(body:unknown)=>request<TrainingSession>('/api/training/sessions',json(body))
export const updateSession=(id:string,body:unknown)=>request<TrainingSession>(`/api/training/sessions/${id}`,json(body,'PUT'))
export const duplicateSession=(id:string,body:unknown)=>request<TrainingSession>(`/api/training/sessions/${id}/duplicate`,json(body))
export const copyTrainingWeek=(body:unknown)=>request<CopyTrainingWeekResult>('/api/training/weeks/copy',json(body))
export const addExercise=(sessionId:string,body:unknown)=>request<TrainingSession>(`/api/training/sessions/${sessionId}/exercises`,json(body))
export const updateExercise=(sessionId:string,exerciseId:string,body:unknown)=>request<TrainingSession>(`/api/training/sessions/${sessionId}/exercises/${exerciseId}`,json(body,'PUT'))
export const startSession=(id:string)=>request<TrainingSession>(`/api/training/sessions/${id}/start`,json({}))
export const recordExercise=(sessionId:string,exerciseId:string,body:unknown)=>request<TrainingSession>(`/api/training/sessions/${sessionId}/exercises/${exerciseId}/result`,json(body,'PUT'))
export const completeSession=(id:string,body:unknown)=>request<TrainingSession>(`/api/training/sessions/${id}/complete`,json(body))
export async function completeSessionResilient(id:string,body:unknown,ownerId:string):Promise<{queued:false;data:TrainingSession}|{queued:true}>{try{return{queued:false,data:await completeSession(id,body)}}catch(error){if(navigator.onLine&&!(error instanceof TypeError))throw error;await queueOperation({id:`training-completion:${ownerId}:${id}`,ownerId,kind:'training-completion',url:`/api/training/sessions/${id}/complete`,method:'POST',body});return{queued:true}}}
export const saveFollowUp=(id:string,body:unknown)=>request<TrainingFollowUp>(`/api/training/sessions/${id}/follow-up`,json(body,'PUT'))
export const getTrainingCalendar=(from:string,to:string)=>request<TrainingCalendar>(`/api/training/calendar?from=${from}&to=${to}`)
export const getSchedule=()=>request<ScheduleBlock[]>('/api/training/schedule')
export const saveScheduleBlock=(body:unknown)=>request<ScheduleBlock[]>('/api/training/schedule',json(body))
export async function removeScheduleBlock(id:string){const response=await fetch(`/api/training/schedule/${id}`,{method:'DELETE',credentials:'include'});if(!response.ok)throw new Error('No se pudo quitar el bloque horario.')}
