import { createRouter, createWebHistory } from 'vue-router'
import TodayView from '../views/TodayView.vue'
import AtlasProfileView from '../views/AtlasProfileView.vue'
import LoginView from '../views/LoginView.vue'
import TrainingView from '../views/TrainingView.vue'
import TrainingFollowUpView from '../views/TrainingFollowUpView.vue'
import InsightsView from '../views/InsightsView.vue'
import MeasurementsView from '../views/MeasurementsView.vue'
import CalendarView from '../views/CalendarView.vue'
import KneeView from '../views/KneeView.vue'
import WeeklyView from '../views/WeeklyView.vue'
import PlanningView from '../views/PlanningView.vue'
import LearningView from '../views/LearningView.vue'
import EvaluationView from '../views/EvaluationView.vue'
import ProfessionalReportView from '../views/ProfessionalReportView.vue'
import DataCenterView from '../views/DataCenterView.vue'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/acceso', name: 'login', component: LoginView, meta: { publicLayout: true } },
    { path: '/', name: 'today', component: TodayView, meta: { requiresAuth: true } },
    { path: '/mi-perfil', name: 'atlas-profile', component: AtlasProfileView, meta: { requiresAuth: true } },
    { path: '/entrenamiento', name: 'training', component: TrainingView, meta: { requiresAuth: true } },
    { path: '/respuesta-24h', name: 'training-follow-up', component: TrainingFollowUpView, meta: { requiresAuth: true } },
    { path: '/tendencias', name: 'insights', component: InsightsView, meta: { requiresAuth: true } },
    { path: '/mediciones', name: 'measurements', component: MeasurementsView, meta: { requiresAuth: true } },
    { path: '/agenda', name: 'calendar', component: CalendarView, meta: { requiresAuth: true } },
    { path: '/rodilla', name: 'knee', component: KneeView, meta: { requiresAuth: true } },
    { path: '/resumen-semanal', name: 'weekly', component: WeeklyView, meta: { requiresAuth: true } },
    { path: '/plan', name: 'planning', component: PlanningView, meta: { requiresAuth: true } },
    { path: '/bitacora', name: 'learning', component: LearningView, meta: { requiresAuth: true } },
    { path: '/evaluacion', name: 'evaluation', component: EvaluationView, meta: { requiresAuth: true } },
    { path: '/informe', name: 'professional-report', component: ProfessionalReportView, meta: { requiresAuth: true } },
    { path: '/mis-datos', name: 'my-data', component: DataCenterView, meta: { requiresAuth: true } },
    { path: '/informe-compartido/:token', name: 'shared-professional-report', component: ProfessionalReportView, meta: { publicLayout: true } },
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()
  await auth.ensureSession()

  if (to.meta.requiresAuth && !auth.user) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.name === 'login' && auth.user) {
    return { name: 'today' }
  }
})

export default router
