export interface PricingPlan {
  id: number;
  name: string;
  price: number | null;
  isRecommended: boolean | null;
  children: PricingPlan[] | null;
}

export interface DisplayPricingPlan {
  id: number;
  name: string;
  fullPath: string;
  price: number;
}
