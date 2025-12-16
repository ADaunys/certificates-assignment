import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { DisplayPricingPlan, PricingPlan } from '../models/pricing-plan';
import {
  filterRecommendedPricingPlans,
  sortPricingPlansByPriceDescending,
} from '../services/pricing-plan.service';

@Component({
  selector: 'app-pricing-plans',
  templateUrl: './pricing-plans.component.html',
  styleUrls: ['./pricing-plans.component.css'],
  imports: [CurrencyPipe],
})
export class PricingPlansComponent implements OnInit {
  public plans: DisplayPricingPlan[] = [];
  public loading = true;
  public error: string | null = null;

  private readonly minPrice = 100;
  private readonly maxPrice = 200;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  ngOnInit(): void {
    this.http.get<PricingPlan[]>(this.baseUrl + 'pricingplans').subscribe({
      next: (result) => {
        const filtered = filterRecommendedPricingPlans(
          result,
          this.minPrice,
          this.maxPrice
        );
        this.plans = sortPricingPlansByPriceDescending(filtered);
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to load pricing plans.';
        this.loading = false;
      },
    });
  }
}
