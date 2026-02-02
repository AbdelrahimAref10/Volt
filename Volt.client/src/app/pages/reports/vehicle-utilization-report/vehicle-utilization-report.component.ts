import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AdminReportsClient, VehicleUtilizationReportDto } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-vehicle-utilization-report',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './vehicle-utilization-report.component.html',
  styleUrls: ['../report-page-shared.css', './vehicle-utilization-report.component.css']
})
export class VehicleUtilizationReportComponent implements OnInit {
  data: VehicleUtilizationReportDto[] = [];
  isLoading = false;
  errorMessage = '';

  constructor(
    private router: Router,
    private reportsClient: AdminReportsClient
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.reportsClient.getVehicleUtilizationReport().subscribe({
      next: (list) => {
        this.data = list;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.errorMessage || err.message || 'Failed to load report';
        this.isLoading = false;
      }
    });
  }

  onBack(): void {
    this.router.navigate(['/main/reports']);
  }
}
