module.exports = {
  preset: 'ts-jest',
  testEnvironment: 'jsdom',
  testMatch: ['<rootDir>/**/*.ts'],
  modulePathIgnorePatterns: ['<rootDir>/bin'],
  moduleNameMapper: {
    '\\.(css)$': '<rootDir>/styleMock.js'
  }
};
