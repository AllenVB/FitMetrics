import type {
  ActivityLevel, ExerciseCategory, Gender, GoalType, MealType, MuscleGroup,
} from '../types';

export const genderLabels: Record<Gender, string> = {
  Male: 'Erkek',
  Female: 'Kadın',
  Other: 'Diğer',
};

export const activityLabels: Record<ActivityLevel, string> = {
  Sedentary: 'Hareketsiz',
  Light: 'Hafif Aktif',
  Moderate: 'Orta Aktif',
  Active: 'Aktif',
  VeryActive: 'Çok Aktif',
};

export const goalLabels: Record<GoalType, string> = {
  LoseWeight: 'Kilo Vermek',
  MaintainWeight: 'Kilo Korumak',
  GainMuscle: 'Kas Kazanmak',
};

export const mealLabels: Record<MealType, string> = {
  Breakfast: 'Kahvaltı',
  Lunch: 'Öğle',
  Dinner: 'Akşam',
  Snack: 'Ara Öğün',
};

export const categoryLabels: Record<ExerciseCategory, string> = {
  Cardio: 'Kardiyo',
  Strength: 'Kuvvet',
  Flexibility: 'Esneklik',
  Sports: 'Spor',
};

export const muscleLabels: Record<MuscleGroup, string> = {
  FullBody: 'Tüm Vücut',
  Chest: 'Göğüs',
  Back: 'Sırt',
  Legs: 'Bacak',
  Shoulders: 'Omuz',
  Arms: 'Kol',
  Core: 'Karın',
  Cardio: 'Kardiyo',
};

export const mealOrder: MealType[] = ['Breakfast', 'Lunch', 'Dinner', 'Snack'];

export const genderOptions = Object.entries(genderLabels) as [Gender, string][];
export const activityOptions = Object.entries(activityLabels) as [ActivityLevel, string][];
export const goalOptions = Object.entries(goalLabels) as [GoalType, string][];

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short' });
}

export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString('tr-TR', {
    day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit',
  });
}
