import type{MeasurementOverview,Metric}from'../types/measurement'
async function request<T>(url:string,options?:RequestInit):Promise<T>{const r=await fetch(url,{...options,credentials:'include'});if(!r.ok){const b=await r.json().catch(()=>null)as{message?:string}|null;throw new Error(b?.message??'No se pudo completar la operación.')}return r.json()as Promise<T>}
const json=(body:unknown,method='POST'):RequestInit=>({method,headers:{'Content-Type':'application/json'},body:JSON.stringify(body)})
export const getMeasurements=()=>request<MeasurementOverview>('/api/measurements')
export const createMetric=(body:unknown)=>request<Metric>('/api/measurements/metrics',json(body))
export const saveEntry=(id:string,body:unknown)=>request<Metric>(`/api/measurements/metrics/${id}/entries`,json(body,'PUT'))
