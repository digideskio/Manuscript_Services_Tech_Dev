using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferDesk.Contracts.Manuscript.DTO;
using TransferDesk.DAL.Manuscript.Repositories;
using TransferDesk.DAL.Manuscript.DataContext;
namespace TransferDesk.BAL.Manuscript
{
    public class AdminDashBoardBL
    {
        public AdminDashBoardReposistory _adminDashBoardReposistory { get; set; }

        public AdminDashBoardBL(string conString)
        {
            _adminDashBoardReposistory = new AdminDashBoardReposistory(conString);
        }

        public bool AllocateManuscriptToUser(AdminDashBoardDTO adminDashBoardDTO)
        {
            if (adminDashBoardDTO.JobType.ToLower() == "book")
            {
                return _adminDashBoardReposistory.AllocateAssociateToChapter(adminDashBoardDTO);
            }
            else
            {
                return _adminDashBoardReposistory.AllocateAssociateToMSID(adminDashBoardDTO);
            }

        }

        public bool updateManuscriptLoginDeatils(AdminDashBoardDTO adminDashBoardDTO)
        {
            if (adminDashBoardDTO.JobType.ToLower() == "book")
            {
                return _adminDashBoardReposistory.UnallocateAssociateUserFromChapter(adminDashBoardDTO) ? true : false;
            }
            else
            {
                return _adminDashBoardReposistory.UnallocateAssociateUser(adminDashBoardDTO) ? true : false;

            }
        }

        public bool updateManuscriptLoginDeatilsForHold(AdminDashBoardDTO adminDashBoardDTO)
        {
            if (adminDashBoardDTO.JobType.ToLower() == "book")
            {
                return _adminDashBoardReposistory.OnHoldBookChapter(adminDashBoardDTO) ? true : false;
            }
            else
            {
                return _adminDashBoardReposistory.HoldMSIDForJob(adminDashBoardDTO) ? true : false;
            }
        }
    }
}
