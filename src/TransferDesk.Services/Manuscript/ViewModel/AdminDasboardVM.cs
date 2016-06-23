using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferDesk.Contracts.Manuscript.ComplexTypes.AdminDashBoard;
using TransferDesk.Contracts.Manuscript.Entities;

namespace TransferDesk.Services.Manuscript.ViewModel
{
    public class AdminDasboardVM
    {

        public int CrestIdVM;
        public string ServiceTypeVM;
        public string RoleVM;
        public string JobProcessingStatusVM;
        public string AssociateNameVM;
        public string StatusVm;
        public IEnumerable<pr_AdminDashBoardGetAllOpenJobs_Result> jobsdetails { get; set; }

    }
}
