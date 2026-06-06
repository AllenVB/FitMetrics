import client from './client';
import type {
  AiFoodSuggestion, AuthResponse, BarcodeLookupResult, ChatMessage, ChatResponse, ClientSummary, CoachResponse, Dashboard,
  DailyNutritionSummary, Exercise, Food, InsightsResponse, KnowledgeEntry, MealPhotoResponse,
  MealPlanResponse, MealType, NutritionLog, RegisterRequest, UpdateProfileRequest, User, WaterToday, WeightEntry, WorkoutLog,
} from '../types';

export interface CreateFoodRequest {
  name: string;
  brand?: string | null;
  category?: string | null;
  caloriesPer100g: number;
  proteinPer100g: number;
  carbsPer100g: number;
  fatPer100g: number;
}

export interface CreateNutritionLogRequest {
  foodId: number;
  amountGrams: number;
  mealType: MealType;
  loggedAt?: string | null;
}

export interface CreateWorkoutLogRequest {
  exerciseId: number;
  durationMinutes?: number | null;
  sets?: number | null;
  reps?: number | null;
  weightKg?: number | null;
  performedAt?: string | null;
}

export interface CreateWeightEntryRequest {
  weightKg: number;
  bodyFatPercentage?: number | null;
  recordedAt?: string | null;
}

export const authApi = {
  login: (email: string, password: string) =>
    client.post<AuthResponse>('/auth/login', { email, password }).then((r) => r.data),
  register: (payload: RegisterRequest) =>
    client.post<AuthResponse>('/auth/register', payload).then((r) => r.data),
  me: () => client.get<User>('/auth/me').then((r) => r.data),
};

export const profileApi = {
  get: () => client.get<User>('/profile').then((r) => r.data),
  update: (payload: UpdateProfileRequest) =>
    client.put<User>('/profile', payload).then((r) => r.data),
};

export const nutritionApi = {
  getFoods: (search?: string) =>
    client.get<Food[]>('/nutrition/foods', { params: { search } }).then((r) => r.data),
  createFood: (payload: CreateFoodRequest) =>
    client.post<Food>('/nutrition/foods', payload).then((r) => r.data),
  addLog: (payload: CreateNutritionLogRequest) =>
    client.post<NutritionLog>('/nutrition/logs', payload).then((r) => r.data),
  deleteLog: (id: number) => client.delete(`/nutrition/logs/${id}`),
  getSummary: (date?: string) =>
    client.get<DailyNutritionSummary>('/nutrition/summary', { params: { date } }).then((r) => r.data),
  lookupBarcode: (code: string) =>
    client.get<BarcodeLookupResult>(`/nutrition/barcode/${encodeURIComponent(code)}`).then((r) => r.data),
  search: (q: string) =>
    client.get<AiFoodSuggestion[]>('/nutrition/search', { params: { q } }).then((r) => r.data),
};

export const waterApi = {
  today: () => client.get<WaterToday>('/water/today').then((r) => r.data),
  add: (amountMl: number) => client.post<WaterToday>('/water', { amountMl }).then((r) => r.data),
};

export const workoutApi = {
  getExercises: (search?: string) =>
    client.get<Exercise[]>('/workout/exercises', { params: { search } }).then((r) => r.data),
  addLog: (payload: CreateWorkoutLogRequest) =>
    client.post<WorkoutLog>('/workout/logs', payload).then((r) => r.data),
  deleteLog: (id: number) => client.delete(`/workout/logs/${id}`),
  getLogs: (from?: string, to?: string) =>
    client.get<WorkoutLog[]>('/workout/logs', { params: { from, to } }).then((r) => r.data),
};

export const weightApi = {
  add: (payload: CreateWeightEntryRequest) =>
    client.post<WeightEntry>('/weight', payload).then((r) => r.data),
  history: () => client.get<WeightEntry[]>('/weight').then((r) => r.data),
  remove: (id: number) => client.delete(`/weight/${id}`),
};

export const dashboardApi = {
  get: () => client.get<Dashboard>('/dashboard').then((r) => r.data),
};

export const insightsApi = {
  get: () => client.get<InsightsResponse>('/insights').then((r) => r.data),
};

export interface GenerateMealPlanRequest {
  prompt: string;
  targetCalories?: number | null;
}

export const aiApi = {
  status: () => client.get<{ enabled: boolean }>('/ai/status').then((r) => r.data),
  mealPlan: (payload: GenerateMealPlanRequest) =>
    client.post<MealPlanResponse>('/ai/meal-plan', payload).then((r) => r.data),
  coach: () => client.get<CoachResponse>('/ai/coach').then((r) => r.data),
  analyzeMealPhoto: (imageBase64: string, mediaType: string) =>
    client.post<MealPhotoResponse>('/ai/analyze-meal-photo', { imageBase64, mediaType }).then((r) => r.data),
  chat: (messages: ChatMessage[]) =>
    client.post<ChatResponse>('/ai/chat', { messages }).then((r) => r.data),
  chatHistory: () => client.get<ChatMessage[]>('/ai/chat/history').then((r) => r.data),
  clearChat: () => client.delete('/ai/chat/history'),
};

export interface CreateKnowledgeEntryRequest {
  question: string;
  answer: string;
}

export const knowledgeApi = {
  getAll: () => client.get<KnowledgeEntry[]>('/knowledge').then((r) => r.data),
  create: (payload: CreateKnowledgeEntryRequest) =>
    client.post<KnowledgeEntry>('/knowledge', payload).then((r) => r.data),
  remove: (id: number) => client.delete(`/knowledge/${id}`),
};

export const reportsApi = {
  downloadMonthly: async (year?: number, month?: number): Promise<Blob> => {
    const res = await client.get('/reports/monthly', { params: { year, month }, responseType: 'blob' });
    return res.data as Blob;
  },
};

export const dietitianApi = {
  enroll: () => client.post<User>('/dietitian/enroll').then((r) => r.data),
  clients: () => client.get<ClientSummary[]>('/dietitian/clients').then((r) => r.data),
  addClient: (email: string) =>
    client.post<ClientSummary>('/dietitian/clients', { email }).then((r) => r.data),
  removeClient: (clientId: number) => client.delete(`/dietitian/clients/${clientId}`),
  clientDashboard: (clientId: number) =>
    client.get<Dashboard>(`/dietitian/clients/${clientId}/dashboard`).then((r) => r.data),
  clientInsights: (clientId: number) =>
    client.get<InsightsResponse>(`/dietitian/clients/${clientId}/insights`).then((r) => r.data),
};
