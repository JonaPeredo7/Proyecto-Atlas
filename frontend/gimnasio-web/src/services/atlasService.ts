import type {
  AtlasOverview,
  AtlasProfile,
  DailyCheckIn,
  DailyActivity,
  SaveDailyActivityRequest,
  SaveDailyCheckInRequest,
  SaveDailyPlanDecisionRequest,
  DailyPlanDecision,
  UpdateAtlasProfileRequest,
} from '../types/atlas'
import { queueOperation } from './offlineQueue'

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    ...options,
    credentials: 'include',
    headers: { 'Content-Type': 'application/json', ...options?.headers },
  })
  if (!response.ok) {
    const body = await response.json().catch(() => null) as { message?: string; title?: string } | null
    throw new Error(body?.message ?? body?.title ?? 'No se pudo completar la operación en Atlas.')
  }
  return response.json() as Promise<T>
}

export const getAtlasOverview = () => request<AtlasOverview>('/api/atlas/overview')

export const updateAtlasProfile = (payload: UpdateAtlasProfileRequest) =>
  request<AtlasProfile>('/api/atlas/profile', { method: 'PUT', body: JSON.stringify(payload) })

export const saveDailyCheckIn = (payload: SaveDailyCheckInRequest) =>
  request<DailyCheckIn>('/api/atlas/check-ins/today', { method: 'PUT', body: JSON.stringify(payload) })

export const saveDailyPlanDecision=(payload:SaveDailyPlanDecisionRequest)=>request<DailyPlanDecision>('/api/atlas/decisions/today',{method:'PUT',body:JSON.stringify(payload)})

export async function saveDailyCheckInResilient(payload:SaveDailyCheckInRequest,ownerId:string):Promise<{queued:false;data:DailyCheckIn}|{queued:true}>{try{return{queued:false,data:await saveDailyCheckIn(payload)}}catch(error){if(navigator.onLine&&!(error instanceof TypeError))throw error;await queueOperation({id:`check-in:${ownerId}:${payload.date}`,ownerId,kind:'daily-check-in',url:'/api/atlas/check-ins/today',method:'PUT',body:payload});return{queued:true}}}

export const saveDailyActivity = (payload: SaveDailyActivityRequest, id?: string) =>
  request<DailyActivity>(id ? `/api/atlas/daily-activities/${id}` : '/api/atlas/daily-activities', { method: id ? 'PUT' : 'POST', body: JSON.stringify(payload) })

export const deleteDailyActivity = async (id: string) => {
  const response = await fetch(`/api/atlas/daily-activities/${id}`, { method: 'DELETE', credentials: 'include' })
  if (!response.ok) throw new Error('No se pudo eliminar la actividad diaria.')
}
