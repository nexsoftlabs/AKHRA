import { test, expect } from '@playwright/test'

test('home loads', async ({ page }) => {
  await page.goto('/')
  await expect(page.getByRole('link', { name: /AKHRA/i })).toBeVisible()
  await expect(page.getByRole('heading', { name: /Cinema that respects/i })).toBeVisible()
})

test('browse route loads', async ({ page }) => {
  await page.goto('/browse')
  await expect(page.getByRole('heading', { name: /Browse movies/i })).toBeVisible()
})

test('legal pages load', async ({ page }) => {
  await page.goto('/privacy')
  await expect(page.getByRole('heading', { name: /Privacy Policy/i })).toBeVisible()
  await page.goto('/terms')
  await expect(page.getByRole('heading', { name: /Terms of Service/i })).toBeVisible()
})
