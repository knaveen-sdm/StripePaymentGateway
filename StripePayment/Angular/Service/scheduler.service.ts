import { Injectable } from "@angular/core";
import { CommonService } from "../../core/services";
import { Observable } from "rxjs";

const getQueryParamsFromObject = (filterModal: object): string => {
  let queryParams = "";
  let index = 0;
  for (const key of Object.keys(filterModal)) {
    if (index === 0) queryParams += `?${key}=${filterModal[key]}`;
    else queryParams += `&${key}=${filterModal[key]}`;

    index++;
  }
  return queryParams;
};

@Injectable({
  providedIn: "root"
})
export class SchedulerService {
  getMasterDataURL = "api/MasterData/MasterDataByName";
  getAllURL = "api/PatientAppointments/GetPatientAppointmentList";
  getStaffAndPatientByLocationURL = "api/PatientAppointments/GetStaffAndPatientByLocation";
  getStaffAvailabilityURL = "AvailabilityTemplates/GetStaffAvailabilityWithLocation";
  getMinMaxOfficeTimeURL = "MasterLocations/GetMinMaxOfficeTime";
  getPatientsURL = "Patients/GetPatients";
  checkIsValidAppointmentURL = "api/PatientAppointments/CheckIsValidAppointmentWithLocation";
  checkAuthorizationDetailsURL = "api/PatientAppointments/GetPatientAuthorizationData";
  getDataForSchedulerURL = "api/PatientAppointments/GetDataForSchedulerByPatient";
  createAppointmentURL = "api/PatientAppointments/SavePatientAppointment";
  createAppointmentFromPatientPortalURL = "PatientPortalAppointment/SavePatientAppointment";
  bookNewAppointmentFromPatientPortalURL = "PatientPortalAppointment/BookNewAppointment";
  getAppointmentDetailURL = "api/PatientAppointments/GetAppointmentDetails";
  deleteAppointmentDetailURL = "api/PatientAppointments/DeleteAppointment";
  cancelAppointmentURL = "api/PatientAppointments/CancelAppointments";
  unCancelAppointmentURL = "api/PatientAppointments/ActivateAppointments";
  updateAppointmentStatusURL = "api/PatientAppointments/UpdateAppointmentStatus";
  updateCallDurationURL = 'api/PatientAppointments/UpdateCallDuration';
  bookNewFreeAppointmentFromPatientPortalURL = "PatientPortalAppointment/BookNewFreeAppointment";
  constructor(private commonService: CommonService) {}

  getMasterData(masterData: any): Observable<any> {
    return this.commonService.post(this.getMasterDataURL, masterData);
  }

  getListData(filterModal: any): Observable<any> {
    // ?locationIds=1&fromDate=2018-11-11&toDate=2018-11-17&staffIds=3
    const queryParams = getQueryParamsFromObject(filterModal);
    return this.commonService.getAll(this.getAllURL + queryParams, {});
  }

  getStaffAndPatientByLocation(
    locationIds: string,
    permissionKey: string = "SCHEDULING_LIST_VIEW_OTHERSTAFF_SCHEDULES"
  ): Observable<any> {
    const queryParams = `?locationIds=${locationIds}&permissionKey=${permissionKey}&isActiveCheckRequired=YES`;
    return this.commonService.getAll(
      this.getStaffAndPatientByLocationURL + queryParams,
      {}
    );
  }

  getStaffAvailability(staffIds: string, locationId: string): Observable<any> {
    const queryParams = `?staffId=${staffIds}&locationId=${locationId}&isLeaveNeeded=true`;
    return this.commonService.getAll(
      this.getStaffAvailabilityURL + queryParams,
      {}
    );
  }

  getMinMaxOfficeTime(locationIds: string): Observable<any> {
    const queryParams = `?locationIds=${locationIds}`;
    return this.commonService.getAll(
      this.getMinMaxOfficeTimeURL + queryParams,
      {}
    );
  }

  getPatientsByLocation(
    searchText: string,
    locationIds: string
  ): Observable<any> {
    // http://108.168.203.227/HC_Patient_Merging/Patients/GetPatients
    const queryParams = `?searchKey=${searchText}&locationIDs=${locationIds}&pageSize=5&isActive=true`;
    return this.commonService.getAll(this.getPatientsURL + queryParams, {});
  }

  getDataForScheduler(filterModal: any): Observable<any> {
    const queryParams = getQueryParamsFromObject(filterModal);
    return this.commonService.getAll(
      this.getDataForSchedulerURL + queryParams,
      {}
    );
  }

  checkIsValidAppointment(appointmentData: any): Observable<any> {
    const postJson = appointmentData;
    return this.commonService.post(this.checkIsValidAppointmentURL, postJson);
  }

  checkAuthorizationDetails(filterModal: any): Observable<any> {
    const queryParams = getQueryParamsFromObject(filterModal);
    return this.commonService.getAll(
      this.checkAuthorizationDetailsURL + queryParams,
      {}
    );
  }

  createAppointment(appointmentData: any, params: any): Observable<any> {
    const queryParams = getQueryParamsFromObject(params);
    const postJson = appointmentData;
    return this.commonService.post(
      this.createAppointmentURL + queryParams,
      postJson
    );
  }

  createAppointmentFromPatientPortal(appointmentData: any): Observable<any> {
    const postJson = appointmentData;
    return this.commonService.post(
      this.createAppointmentFromPatientPortalURL,
      postJson
    );
  }
  bookNewAppointmentFromPatientPortal(appointmentData: any): Observable<any> {
    const postJson = appointmentData;
    return this.commonService.post(
      this.bookNewAppointmentFromPatientPortalURL,
      postJson
    );
  }

  bookNewFreeAppointmentFromPatientPortal(appointmentData: any): Observable<any> {
    const postJson = appointmentData;
    return this.commonService.post(
      this.bookNewFreeAppointmentFromPatientPortalURL,
      postJson
    );
  }

  getAppointmentDetails(appointmentId: number): Observable<any> {
    const queryParams = `?appointmentId=${appointmentId}`;
    return this.commonService.getById(
      this.getAppointmentDetailURL + queryParams,
      {}
    );
  }

  deleteAppointmentDetails(
    appointmentId: number,
    parentAppointmentId: number,
    deleteSeries: boolean,
    isAdmin: boolean
  ): Observable<any> {
    const queryParams = `?appointmentId=${appointmentId}&parentAppointmentId=${parentAppointmentId}&deleteSeries=${deleteSeries}&isAdmin=${isAdmin}`;
    return this.commonService.patch(
      this.deleteAppointmentDetailURL + queryParams,
      {}
    );
  }

  cancelAppointment(appointmentData: any): Observable<any> {
    return this.commonService.post(this.cancelAppointmentURL, appointmentData);
  }

  unCancelAppointment(appointmentId: number): Observable<any> {
    const queryParams = `?appointmentId=${appointmentId}`;
    return this.commonService.patch(
      this.unCancelAppointmentURL + queryParams,
      {}
    );
  }

  updateAppointmentStatus(appointmentData: any): Observable<any> {
    return this.commonService.post(
      this.updateAppointmentStatusURL,
      appointmentData
    );
  }

  getUserScreenActionPermissions(moduleName: string, screenName: string): any {
    return this.commonService.getUserScreenActionPermissions(
      moduleName,
      screenName
    );
  }
  UpdateCallDuration(appointmentId: number,timeDuration:string): Observable<any> {
    const queryParams = `?appointmentId=${appointmentId}&timeDuration=${timeDuration}`;
    return this.commonService.post(this.updateCallDurationURL+ queryParams,{});
  }

}
