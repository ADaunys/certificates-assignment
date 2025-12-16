import { PricingPlan, DisplayPricingPlan } from '../models/pricing-plan';
import {
  flattenPricingPlans,
  filterRecommendedPricingPlans,
  sortPricingPlansByPriceDescending,
} from './pricing-plan.service';

describe('PricingPlanService', () => {
  const mockPricingPlans: PricingPlan[] = [
    {
      id: 10,
      name: 'All plans',
      price: null,
      isRecommended: null,
      children: [
        {
          id: 20,
          name: 'Basic',
          price: null,
          isRecommended: null,
          children: [
            { id: 30, name: 'Student', price: 110, isRecommended: true, children: null },
            { id: 40, name: 'Individual', price: 105, isRecommended: false, children: null },
          ],
        },
        {
          id: 60,
          name: 'Standard',
          price: null,
          isRecommended: null,
          children: [
            { id: 70, name: 'Family', price: 180, isRecommended: false, children: null },
            { id: 80, name: 'Family Plus', price: 130, isRecommended: true, children: null },
          ],
        },
        {
          id: 100,
          name: 'Premium',
          price: null,
          isRecommended: null,
          children: [
            { id: 110, name: 'Business', price: 250, isRecommended: true, children: null },
            { id: 120, name: 'Individual', price: 170, isRecommended: true, children: null },
          ],
        },
      ],
    },
  ];

  describe('flattenPricingPlans', () => {
    it('should flatten hierarchical pricing plans into flat list', () => {
      const result = flattenPricingPlans(mockPricingPlans);

      expect(result.length).toBe(6);
      expect(result.every((plan) => plan.price !== null)).toBe(true);
    });

    it('should build full path for each plan', () => {
      const result = flattenPricingPlans(mockPricingPlans);

      const studentPlan = result.find((p) => p.id === 30);
      expect(studentPlan?.fullPath).toBe('All plans / Basic / Student');

      const familyPlusPlan = result.find((p) => p.id === 80);
      expect(familyPlusPlan?.fullPath).toBe('All plans / Standard / Family Plus');
    });

    it('should return empty array for empty input', () => {
      const result = flattenPricingPlans([]);

      expect(result).toEqual([]);
    });
  });

  describe('filterRecommendedPricingPlans', () => {
    it('should filter only recommended plans within price range', () => {
      const result = filterRecommendedPricingPlans(mockPricingPlans, 100, 200);

      expect(result.length).toBe(4);
      expect(result.every((plan) => plan.price >= 100 && plan.price <= 200)).toBe(true);
    });

    it('should exclude non-recommended plans', () => {
      const result = filterRecommendedPricingPlans(mockPricingPlans, 100, 200);

      const individualBasic = result.find((p) => p.id === 40);
      expect(individualBasic).toBeUndefined();

      const family = result.find((p) => p.id === 70);
      expect(family).toBeUndefined();
    });

    it('should exclude plans outside price range', () => {
      const result = filterRecommendedPricingPlans(mockPricingPlans, 100, 200);

      const business = result.find((p) => p.id === 110);
      expect(business).toBeUndefined();
    });

    it('should include plan at exact minimum price', () => {
      const result = filterRecommendedPricingPlans(mockPricingPlans, 110, 200);

      const student = result.find((p) => p.id === 30);
      expect(student).toBeDefined();
      expect(student?.price).toBe(110);
    });

    it('should include plan at exact maximum price', () => {
      const result = filterRecommendedPricingPlans(mockPricingPlans, 100, 170);

      const individual = result.find((p) => p.id === 120);
      expect(individual).toBeDefined();
      expect(individual?.price).toBe(170);
    });
  });

  describe('sortPricingPlansByPriceDescending', () => {
    it('should sort plans by price in descending order', () => {
      const plans: DisplayPricingPlan[] = [
        { id: 1, name: 'Low', fullPath: 'Low', price: 100 },
        { id: 2, name: 'High', fullPath: 'High', price: 200 },
        { id: 3, name: 'Medium', fullPath: 'Medium', price: 150 },
      ];

      const result = sortPricingPlansByPriceDescending(plans);

      expect(result[0].price).toBe(200);
      expect(result[1].price).toBe(150);
      expect(result[2].price).toBe(100);
    });

    it('should not mutate the original array', () => {
      const plans: DisplayPricingPlan[] = [
        { id: 1, name: 'Low', fullPath: 'Low', price: 100 },
        { id: 2, name: 'High', fullPath: 'High', price: 200 },
      ];

      const result = sortPricingPlansByPriceDescending(plans);

      expect(result).not.toBe(plans);
      expect(plans[0].price).toBe(100);
    });

    it('should handle empty array', () => {
      const result = sortPricingPlansByPriceDescending([]);

      expect(result).toEqual([]);
    });

    it('should handle single element array', () => {
      const plans: DisplayPricingPlan[] = [
        { id: 1, name: 'Single', fullPath: 'Single', price: 100 },
      ];

      const result = sortPricingPlansByPriceDescending(plans);

      expect(result.length).toBe(1);
      expect(result[0].price).toBe(100);
    });
  });
});
