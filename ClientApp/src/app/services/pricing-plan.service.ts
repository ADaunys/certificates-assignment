import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { DisplayPricingPlan, PricingPlan } from '../models/pricing-plan';

export interface PricingPlanFilter {
  minPrice: number;
  maxPrice: number;
  recommendedOnly: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class PricingPlanService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  getRecommendedPlans(filter: PricingPlanFilter): Observable<DisplayPricingPlan[]> {
    return this.http.get<PricingPlan[]>(this.baseUrl + 'pricingplans').pipe(
      map((plans) => {
        const flattened = flattenPricingPlans(plans);
        const filtered = filterPricingPlans(flattened, filter);
        return sortPricingPlansByPriceDescending(filtered);
      })
    );
  }
}

export function flattenPricingPlans(
  plans: PricingPlan[],
  parentPath: string = ''
): DisplayPricingPlan[] {
  const result: DisplayPricingPlan[] = [];

  for (const plan of plans) {
    const currentPath = parentPath ? `${parentPath} / ${plan.name}` : plan.name;

    if (plan.price !== null) {
      result.push({
        id: plan.id,
        name: plan.name,
        fullPath: currentPath,
        price: plan.price,
      });
    }

    if (plan.children && plan.children.length > 0) {
      result.push(...flattenPricingPlans(plan.children, currentPath));
    }
  }

  return result;
}

export function filterPricingPlans(
  plans: DisplayPricingPlan[],
  filter: PricingPlanFilter,
  originalPlans?: PricingPlan[]
): DisplayPricingPlan[] {
  return plans.filter((plan) => {
    const withinPriceRange = plan.price >= filter.minPrice && plan.price <= filter.maxPrice;

    if (!filter.recommendedOnly) {
      return withinPriceRange;
    }

    return withinPriceRange && isRecommended(plan.id, originalPlans);
  });
}

export function filterRecommendedPricingPlans(
  plans: PricingPlan[],
  minPrice: number,
  maxPrice: number,
  parentPath: string = ''
): DisplayPricingPlan[] {
  const result: DisplayPricingPlan[] = [];

  for (const plan of plans) {
    const currentPath = parentPath ? `${parentPath} / ${plan.name}` : plan.name;

    if (
      plan.price !== null &&
      plan.isRecommended === true &&
      plan.price >= minPrice &&
      plan.price <= maxPrice
    ) {
      result.push({
        id: plan.id,
        name: plan.name,
        fullPath: currentPath,
        price: plan.price,
      });
    }

    if (plan.children && plan.children.length > 0) {
      result.push(
        ...filterRecommendedPricingPlans(plan.children, minPrice, maxPrice, currentPath)
      );
    }
  }

  return result;
}

export function sortPricingPlansByPriceDescending(
  plans: DisplayPricingPlan[]
): DisplayPricingPlan[] {
  return [...plans].sort((a, b) => b.price - a.price);
}

function isRecommended(planId: number, plans?: PricingPlan[]): boolean {
  if (!plans) return true;

  for (const plan of plans) {
    if (plan.id === planId) {
      return plan.isRecommended === true;
    }
    if (plan.children) {
      const found = isRecommended(planId, plan.children);
      if (found) return true;
    }
  }
  return false;
}
