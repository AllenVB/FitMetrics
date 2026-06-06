// Backend DTO/enum'larını birebir yansıtan TypeScript tipleri.
// Enum'lar backend'de string olarak serileştirilir.

export type Gender = 'Male' | 'Female' | 'Other';
export type ActivityLevel = 'Sedentary' | 'Light' | 'Moderate' | 'Active' | 'VeryActive';
export type GoalType = 'LoseWeight' | 'MaintainWeight' | 'GainMuscle';
export type MealType = 'Breakfast' | 'Lunch' | 'Dinner' | 'Snack';
export type ExerciseCategory = 'Cardio' | 'Strength' | 'Flexibility' | 'Sports';
export type MuscleGroup =
  | 'FullBody' | 'Chest' | 'Back' | 'Legs' | 'Shoulders' | 'Arms' | 'Core' | 'Cardio';
export type InsightSeverity = 'Positive' | 'Info' | 'Warning';
export type InsightCategory =
  | 'Calories' | 'Protein' | 'Macros' | 'Workout' | 'Weight' | 'Consistency';

export type UserRole = 'Member' | 'Dietitian';

export interface User {
  id: number;
  fullName: string;
  email: string;
  role: UserRole;
  age: number;
  gender: Gender;
  heightCm: number;
  currentWeightKg: number;
  activityLevel: ActivityLevel;
  goalType: GoalType;
  targetWeightKg?: number | null;
  dailyCalorieGoal: number;
  dailyProteinGoal: number;
  dailyWaterGoalMl: number;
  bmi: number;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: User;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  age: number;
  gender: Gender;
  heightCm: number;
  currentWeightKg: number;
  activityLevel: ActivityLevel;
  goalType: GoalType;
  targetWeightKg?: number | null;
}

export interface UpdateProfileRequest {
  fullName: string;
  age: number;
  gender: Gender;
  heightCm: number;
  currentWeightKg: number;
  activityLevel: ActivityLevel;
  goalType: GoalType;
  targetWeightKg?: number | null;
  dailyCalorieGoal?: number | null;
  dailyProteinGoal?: number | null;
  dailyWaterGoalMl?: number | null;
}

export interface Food {
  id: number;
  name: string;
  brand?: string | null;
  category?: string | null;
  caloriesPer100g: number;
  proteinPer100g: number;
  carbsPer100g: number;
  fatPer100g: number;
}

export interface NutritionLog {
  id: number;
  foodId: number;
  foodName: string;
  mealType: MealType;
  amountGrams: number;
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
  loggedAt: string;
}

export interface MealGroup {
  mealType: MealType;
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
  items: NutritionLog[];
}

export interface DailyNutritionSummary {
  date: string;
  totalCalories: number;
  totalProtein: number;
  totalCarbs: number;
  totalFat: number;
  calorieGoal: number;
  proteinGoal: number;
  meals: MealGroup[];
}

export interface Exercise {
  id: number;
  name: string;
  category: ExerciseCategory;
  muscleGroup: MuscleGroup;
  caloriesBurnedPerMinute: number;
}

export interface WorkoutLog {
  id: number;
  exerciseId: number;
  exerciseName: string;
  category: ExerciseCategory;
  muscleGroup: MuscleGroup;
  durationMinutes?: number | null;
  sets?: number | null;
  reps?: number | null;
  weightKg?: number | null;
  caloriesBurned: number;
  performedAt: string;
}

export interface WeightEntry {
  id: number;
  weightKg: number;
  bodyFatPercentage?: number | null;
  recordedAt: string;
}

export interface DailyCaloriePoint {
  date: string;
  calories: number;
  goal: number;
  burnedCalories: number;
}

export interface WeightPoint {
  date: string;
  weightKg: number;
  bodyFatPercentage?: number | null;
}

export interface MacroBreakdown {
  protein: number;
  carbs: number;
  fat: number;
}

export interface Dashboard {
  today: DailyNutritionSummary;
  waterGoalMl: number;
  waterIntakeMl: number;
  currentWeightKg?: number | null;
  targetWeightKg?: number | null;
  bmi: number;
  workoutsThisWeek: number;
  caloriesBurnedThisWeek: number;
  todayMacros: MacroBreakdown;
  calorieTrend: DailyCaloriePoint[];
  weightTrend: WeightPoint[];
}

export interface Insight {
  category: InsightCategory;
  severity: InsightSeverity;
  title: string;
  message: string;
  metric?: string | null;
}

export interface InsightsResponse {
  generatedAt: string;
  daysAnalyzed: number;
  insights: Insight[];
}

// ---- AI (Claude) ----

export interface MealPlanFood { name: string; amount: string; calories: number; protein: number; }
export interface MealPlanMeal { mealType: string; calories: number; foods: MealPlanFood[]; }
export interface MealPlanResponse {
  summary: string;
  totalCalories: number;
  totalProtein: number;
  meals: MealPlanMeal[];
}

export interface CoachResponse { message: string; focusAreas: string[]; }

export interface DetectedFood { name: string; portion: string; calories: number; protein: number; }
export interface MealPhotoResponse {
  description: string;
  foods: DetectedFood[];
  estimatedCalories: number;
  estimatedProtein: number;
  estimatedCarbs: number;
  estimatedFat: number;
}

// ---- Barkod & Diyetisyen ----

export interface BarcodeLookupResult {
  barcode: string;
  name: string;
  brand?: string | null;
  caloriesPer100g: number;
  proteinPer100g: number;
  carbsPer100g: number;
  fatPer100g: number;
}

export interface ClientSummary {
  id: number;
  fullName: string;
  email: string;
  goalType: GoalType;
  currentWeightKg: number;
  bmi: number;
  linkedAt: string;
}

export interface FatSecretFoodResult {
  id: string;
  name: string;
  brand?: string | null;
  description: string;
}

/** AI (LLM) besin arama önerisi — 100g başına tahmini değerler. */
export interface AiFoodSuggestion {
  name: string;
  brand?: string | null;
  description: string;
  caloriesPer100g: number;
  proteinPer100g: number;
  carbsPer100g: number;
  fatPer100g: number;
}

export interface WaterToday {
  intakeMl: number;
  goalMl: number;
}

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

export interface ChatResponse {
  reply: string;
}

// ---- Bilgi Tabanı (AI grounding) ----

export interface KnowledgeEntry {
  id: number;
  question: string;
  answer: string;
  createdAt: string;
}
